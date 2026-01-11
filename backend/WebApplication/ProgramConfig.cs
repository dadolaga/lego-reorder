using Microsoft.AspNetCore.Hosting.Server;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication {
    internal class ProgramConfig {
        const string BASE_SETTING_NAME = "appsettings.jsonc";
        public static MainConfig Config { get; private set; }

        public static void ReadConfig(string? configPath = null) {
            var jsonSerializerOptions = new JsonSerializerOptions {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            try {
                if (configPath == null) {
                    configPath = Path.Combine(Directory.GetCurrentDirectory(), BASE_SETTING_NAME);
                }

                var config = JsonSerializer.Deserialize<MainConfig>(File.ReadAllText(configPath), jsonSerializerOptions);

                if (config != null)
                    Config = config;
            } catch (Exception ex) {
                Console.WriteLine($"Fail when try to read config file path: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }

    internal class MainConfig {
        [JsonPropertyName("database")]
        public DatabaseConfig Database { get; set; }

        [JsonPropertyName("lego-api")]
        public LegoApiConfig LegoApiConfig { get; set; }
    }

    internal class DatabaseConfig {
        [JsonPropertyName("server")]
        public string Server { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    internal class LegoApiConfig {

        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
