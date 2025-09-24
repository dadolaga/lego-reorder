using Serilog;

namespace application {
    internal class Program {
        static void Main(string[] args) {
            Logger.Init();

            Log.Information("Lego reorder application starting...");
        }
    }
}
