using System.Runtime.CompilerServices;
using System.Text.Json;

namespace LegoApi {
    public class LegoInitializer {
        public static void Init() {
            string jsonPath = Path.Combine(Path.GetDirectoryName(Directory.GetCurrentDirectory()), "appsettings.json");

            try {
                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(jsonPath));

                LegoApiFactory.Token = json["LegoApiToken"];
            } catch (FileNotFoundException) {
                MyLogger.Log.Warning("App setting file not found");
            }
        }
    }
}
