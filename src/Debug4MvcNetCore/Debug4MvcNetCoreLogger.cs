using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Debug4MvcNetCore
{
    public class Debug4MvcNetCoreLogger : ILogger
    {
        private readonly string _name;
        private LogsService _logsService = new LogsService();

        public Debug4MvcNetCoreLogger(string name)
        {
            _name = name;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == _logsService.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (_logsService.EventId == 0 || _logsService.EventId == eventId.Id)
            {
                _logsService.AddLog(_name, logLevel, eventId, state, exception, formatter);
            }
        }
    }

    public class Debug4MvcNetCoreLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, Debug4MvcNetCoreLogger> _loggers = new ConcurrentDictionary<string, Debug4MvcNetCoreLogger>();

        public Debug4MvcNetCoreLoggerProvider()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new Debug4MvcNetCoreLogger(name));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }


}
