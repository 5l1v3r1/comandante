using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Debug4MvcNetCore
{
    public class LogEntry
    {
        public LogLevel LogLevel { get; internal set; }
        public int EventId { get; internal set; }
        public object LoggerName { get; internal set; }
        public string Details { get; internal set; }
        public string IdentityName { get; internal set; }
        public string TraceIdentifier { get; internal set; }
    }

    public class LogsService
    {
        public LogsService()
        {

        }

        private static LogLevel _logLevel = LogLevel.Information;
        public LogLevel LogLevel
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        private static int _eventId = 0;
        public int EventId
        {
            get { return _eventId; }
            set { _eventId = value; }
        }

        private static List<LogEntry> _logs = new List<LogEntry>();
        public IEnumerable<LogEntry> Logs
        {
            get { return _logs; }
        }

        private static Dictionary<string, DebugInfo> _requests = new Dictionary<string, DebugInfo>();
        public IEnumerable<DebugInfo> Requests
        {
            get { return _requests.Select(x => x.Value).OrderByDescending(x => x.Created); }
        }

        public void AddLog<TState>(string loggerName, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var httpContext = new HttpContextService().HttpContext;
            _logs.Insert(0, new LogEntry
            {
                IdentityName = httpContext?.User?.Identity?.Name,
                TraceIdentifier = httpContext?.TraceIdentifier,
                LogLevel = logLevel,
                EventId = eventId.Id,
                LoggerName = loggerName,
                Details = formatter(state, exception)
            });
        }

        public void AddRequest(HttpContext httpContext)
        {
            if (_requests.ContainsKey(httpContext.TraceIdentifier) == false)
                _requests.Add(httpContext.TraceIdentifier, new DebugInfoService().Create(httpContext));
        }
    }
}
