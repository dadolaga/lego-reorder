namespace WebApplication {
    public class LoggerProvider : ILoggerProvider {
        public ILogger CreateLogger(string categoryName) {
            return new Logger();
        }

        public void Dispose() {
            
        }
    }

    public class Logger : ILogger {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull { 
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            switch (logLevel) {
                case LogLevel.Critical:
                    MyLogger.Log.Fatal(formatter(state, exception));
                    break;
                case LogLevel.Error:
                    MyLogger.Log.Error(formatter(state, exception));
                    break;
                case LogLevel.Warning:
                    MyLogger.Log.Warning(formatter(state, exception));
                    break;
                case LogLevel.Information:
                    MyLogger.Log.Information(formatter(state, exception));
                    break;
                case LogLevel.Debug:
                    MyLogger.Log.Debug(formatter(state, exception));
                    break;
                default:
                    MyLogger.Log.Information(formatter(state, exception));
                    break;
            }
        }
    }
}
