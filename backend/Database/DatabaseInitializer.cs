using System.Text.Json;

namespace Database {
    public class DatabaseInitializer {
        internal static string ConnectionString { get; private set; }

        public static void Init(string server, string name, string user, string password, int port = 3306) {
            MyLogger.Log.Information("Initialize DB");

            ConnectionString = $"Server={server};Port={port};Database={name};Uid={user};Pwd={password};";

            using (var db = new LegoDbContext()) {
                try {
                    db.Database.EnsureCreated();

                    MyLogger.Log.Information("Database Initialize correctly");
                } catch (Exception ex) {
                    MyLogger.Log.Error($"Database creation throws an error: {ex.Message}");
                }
            }
        }
    }
}
