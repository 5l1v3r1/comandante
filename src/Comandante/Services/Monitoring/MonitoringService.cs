using Comandante.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Comandante
{
    public class MonitoringService
    {
        private static ConcurrentDictionary<RequestResponseInfo, RequestResponseInfo> _requestsEnded = new ConcurrentDictionary<RequestResponseInfo, RequestResponseInfo>();
        public IEnumerable<RequestResponseInfo> RequestsEnded
        {
            get { return _requestsEnded.ToArray().Select(x => x.Key); }
        }

        private static ConcurrentDictionary<RequestResponseInfo, RequestResponseInfo> _requestsActive = new ConcurrentDictionary<RequestResponseInfo, RequestResponseInfo>();
        public IEnumerable<RequestResponseInfo> RequestsActive
        {
            get { return _requestsActive.ToArray().Select(x => x.Key); }
        }

        public IEnumerable<RequestResponseInfo> AllRequests
        {
            get
            {
                return
                  RequestsActive
                  .Union(RequestsEnded)
                  .OrderByDescending(x => x.Started)
                  .ToList();
            }
        }

        private static ConcurrentQueue<LogEntry> _webHostlogs = new ConcurrentQueue<LogEntry>();
        public IEnumerable<LogEntry> WebHostlogs
        {
            get { return _webHostlogs.ToArray(); }
        }

        public IEnumerable<LogEntry> AllLogs
        {
            get
            {
                return
                  WebHostlogs
                  .Union(RequestsActive.SelectMany(x => x.Logs.ToArray().Where(y => y != null)))
                  .Union(RequestsEnded.SelectMany(x => x.Logs.ToArray().Where(y => y != null)))
                  .OrderByDescending(x => x.Created)
                  .ToList();
            }
        }

        public void StartRequest(HttpContext httpContext)
        {
            if (ConfigurationInfo.EnableRequestLogs == false)
                return;

            var request = new RequestResponseInfoAssembler().CreateRequestResponseInfo(httpContext);
            new HttpContextHelper().RequestResponseInfo = request;
            _requestsActive.TryAdd(request, request);
        }

        public void EndRequest(HttpContext httpContext)
        {
            if (ConfigurationInfo.EnableRequestLogs == false)
                return;

            LogUnhandledException(httpContext);

            var request = new HttpContextHelper().RequestResponseInfo;
            request.Response = new RequestResponseInfoAssembler().CreateResponseInfo(httpContext);
            request.Identities = new RequestResponseInfoAssembler().CreateIdentityInfo(httpContext);
            request.Completed = DateTime.UtcNow;

            if (ConfigurationInfo.EnableRequestLogsOnlyIfAspMvc && request.IsAspNetCore == false)
            {
                _requestsActive.TryRemove(request, out RequestResponseInfo removeingItem1);
                return;
            }
            if (ConfigurationInfo.EnableRequestLogsOnlyIfError && request.IsError == false)
            {
                _requestsActive.TryRemove(request, out RequestResponseInfo removeingItem2);
                return;
            }

            _requestsEnded.TryAdd(request, request);
            _requestsActive.TryRemove(request, out RequestResponseInfo removeingItem3);
            CleanUpRequests();
        }

        private static object _cleanUpRequestsMonitor = new object();
        private static bool _cleanUpRequestsMonitorLocked = false;
        public void CleanUpRequests()
        {
            if (_cleanUpRequestsMonitorLocked)
                return;

            Task.Run(() =>
            {
                if (Monitor.TryEnter(_cleanUpRequestsMonitor))
                {
                    try
                    {
                        _cleanUpRequestsMonitorLocked = true;
                        if (_requestsEnded.Count > ConfigurationInfo.MaxNumberOfRequestsLogs)
                        {
                            foreach (var request in _requestsEnded.OrderByDescending(x => x.Key.Started).Skip(ConfigurationInfo.MaxNumberOfRequestsLogs))
                                _requestsEnded.TryRemove(request.Key, out RequestResponseInfo removingItem);
                        }
                    }
                    finally
                    {
                        _cleanUpRequestsMonitorLocked = false;
                        Monitor.Exit(_cleanUpRequestsMonitor);
                    }
                }
            });
        }

        public void LogUnhandledException(HttpContext httpContext)
        {
            var exceptionHandlerPathFeature = httpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                var logger = (ILogger<ComandanteMiddleware>)httpContext.RequestServices.GetService(typeof(ILogger<ComandanteMiddleware>));
                if (logger != null)
                    logger.LogError(exceptionHandlerPathFeature.Error.Message, exceptionHandlerPathFeature.Error);
            }
        }

        public void AddLog<TState>(string loggerName, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var httpContext = new HttpContextHelper().HttpContext;
            bool isWebHostLog = httpContext == null;

            if (isWebHostLog && ConfigurationInfo.EnableWebHostLogs == false)
                return;

            var details = formatter(state, exception);
            var exceptionDetails = "";
            while (exception != null)
            {
                details = details + " \n " + exception.Message;
                exceptionDetails = exceptionDetails + (exception.Message + " \n " + exception.StackTrace);
                exception = exception.InnerException;
            }

            var logEntry = new LogEntry
            {
                Id = Guid.NewGuid().ToString(),
                IdentityName = httpContext?.User?.Identity?.Name,
                TraceIdentifier = httpContext?.TraceIdentifier,
                LogLevel = logLevel,
                EventId = eventId.Id,
                LoggerName = loggerName,
                Details = details,
                Exception = exceptionDetails,
                Created = DateTime.UtcNow,
                RequestUrl = string.Concat(httpContext?.Request?.Method, " ", httpContext?.Request?.Path),
            };

            if (isWebHostLog)
            {
                if (_webHostlogs.Count > ConfigurationInfo.MaxNumberOfWebHostLogs)
                    _webHostlogs.TryDequeue(out LogEntry remove);
                _webHostlogs.Enqueue(logEntry);
            }
            else
            {
                var requestInfo = new HttpContextHelper().RequestResponseInfo;
                if (requestInfo != null)
                {
                    requestInfo.Logs.Insert(0, logEntry);
                }
                else
                {
                    if (_webHostlogs.Count > ConfigurationInfo.MaxNumberOfWebHostLogs)
                        _webHostlogs.TryDequeue(out LogEntry remove);
                    _webHostlogs.Enqueue(logEntry);
                }
            }
        }

        public void ClearLogs()
        {
            _webHostlogs.Clear();
            _requestsEnded.Clear();
        }

       
    }
}
