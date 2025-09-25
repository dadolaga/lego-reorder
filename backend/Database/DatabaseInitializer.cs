using System.Text.Json;

namespace Database {
    internal class JsonInitializer {
        public string ConnectionString { get; set;}

    }

    public class DatabaseInitializer {
        public static void ReadFromJson() {
            string jsonPath = Path.Combine(Path.GetDirectoryName(Directory.GetCurrentDirectory()), "appsettings.json");

            JsonInitializer? json = JsonSerializer.Deserialize<JsonInitializer>(File.ReadAllText(jsonPath));

            if(json == null) { 
                MyLogger.Log.Fatal("Database json initializer file not read");
                return;
            }

            LegoDbContext.ConnectionString = json.ConnectionString;
        }
    }
}
