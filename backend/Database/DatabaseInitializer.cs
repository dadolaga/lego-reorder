using System.Text.Json;

namespace Database {
    internal class JsonInitializer {
        public string ConnectionString { get; set;}

    }

    public class DatabaseInitializer {
        public static void ReadFromJson() {
            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            JsonInitializer? json = JsonSerializer.Deserialize<JsonInitializer>(File.ReadAllText(jsonPath));

            if(json == null) { 
                MyLogger.Log.Fatal("Database json initializer file not read");
                return;
            }

            LegoDbContext.ConnectionString = json.ConnectionString;
        }

        public static void Init() {
            MyLogger.Log.Information("Initalize DB");

            using (var db = new LegoDbContext()) {
                try {
                    db.Database.EnsureCreated();

                    MyLogger.Log.Information("Database initalize correctly");
                } catch (Exception ex) {
                    MyLogger.Log.Error($"Database creation trows an error: {ex.Message}");
                }
            }
        }
    }
}
