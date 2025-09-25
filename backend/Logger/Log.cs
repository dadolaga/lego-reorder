
using Serilog;

namespace MyLogger {
    public class Log : ILog {
        private static bool init_ = false;

        public static void Init() {
            Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u4}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

            init_ = true;
        }

        public static void Debug(string text) {
            if(!init_)
                return;

            Serilog.Log.Debug(text);
        }

        public static void Error(string text, Exception? exception = null) {
            if (!init_)
                return;

            Serilog.Log.Error(text, exception);
        }

        public static void Fatal(string text, Exception? exception = null) {
            if (!init_)
                return;

            Serilog.Log.Fatal(text, exception);
        }

        public static void Information(string text) {
            if (!init_)
                return;

            Serilog.Log.Information(text);
        }

        public static void Warning(string text, Exception? exception = null) {
            if (!init_)
                return;

            Serilog.Log.Warning(text, exception);
        }

    }
}
