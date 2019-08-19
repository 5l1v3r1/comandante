using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Comandante
{
    public class ComandanteLogger : ILogger
    {
        private readonly string _name;
        private RequestsService _requestsService = new RequestsService();

        public ComandanteLogger(string name)
        {
            _name = name;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _requestsService.AddLog(_name, (LogLevel)(int)logLevel, eventId, state, exception, formatter);
        }
    }

    public class ComandanteLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ComandanteLogger> _loggers = new ConcurrentDictionary<string, ComandanteLogger>();

        public ComandanteLoggerProvider()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new ComandanteLogger(name));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }


}
