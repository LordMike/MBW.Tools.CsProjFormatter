using System;
using Microsoft.Extensions.Logging;

namespace MBW.Tools.CsProjFormatter.Library.Utilities
{
    internal sealed class PrefixingLogger : ILogger
    {
        private ILogger _logger;
        private readonly string _prefix;

        public PrefixingLogger(ILogger logger, string prefix)
        {
            _logger = logger;
            _prefix = prefix;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, (state1, exception1) =>
            {
                string res = formatter(state1, exception);

                return _prefix + res;
            });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }
    }
}
