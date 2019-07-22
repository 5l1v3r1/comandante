using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Debug4MvcNetCore
{
    public class LogsService
    {
        public LogsService()
        {
        }

        private static LogLevel _logLevel = LogLevel.EverythingIfError;
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

        private static bool _ignoreWebHost = true;
        public bool IgnoreWebHost
        {
            get { return _ignoreWebHost; }
            set { _ignoreWebHost = value; }
        }

        public void AddLog<TState>(string loggerName, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if ((int)logLevel < (int)LogLevel)
                return;

            if (EventId != 0 && EventId != eventId.Id)
                return;

            if (IgnoreWebHost && loggerName == "Microsoft.AspNetCore.Hosting.Internal.WebHost")
                return;

            var httpContext = new HttpContextHelper().HttpContext;
            _logs.Insert(0, new LogEntry
            {
                IdentityName = httpContext?.User?.Identity?.Name,
                TraceIdentifier = httpContext?.TraceIdentifier,
                LogLevel = logLevel,
                EventId = eventId.Id,
                LoggerName = loggerName,
                Details = formatter(state, exception)
            });
            
            //Keep only 2000 logs
            _logs = _logs.Take(2000).ToList();
        }

        public void ClearLogs()
        {
            _logs.Clear();
        }

        public void CleanUp(HttpContext context)
        {
            if (LogLevel == LogLevel.EverythingIfError)
            {
                var logs = _logs.Where(x => x.TraceIdentifier == context.TraceIdentifier).ToList();
                if (logs.All(x => x.LogLevel < LogLevel.Error))
                {
                    foreach(var log in logs)
                        _logs.Remove(log);
                }
            }
        }
    }

    public class LogEntry
    {
        public LogLevel LogLevel { get; internal set; }
        public int EventId { get; internal set; }
        public object LoggerName { get; internal set; }
        public string Details { get; internal set; }
        public string IdentityName { get; internal set; }
        public string TraceIdentifier { get; internal set; }
    }

    public enum LogLevel : int
    {
        EverythingIfError = -1,
        None = 6,
        Critical = 5,
        Error = 4,
        Warning = 3,
        Information = 2,
        Debug = 1,
        Trace = 0
    }
}
