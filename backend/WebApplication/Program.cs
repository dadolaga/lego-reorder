
using Database;
using LegoApi;
using MyLogger;
using Serilog;
using System.Net.WebSockets;
using WebApplication.WebSocketController;

namespace WebApplication {
    public class Program {
        public static void Main(string[] args) {
            MyLogger.Log.Init();

            InitializeDatabase();

            LegoInitializer.Init();

            InitializeApi();
        }

        private static void InitializeDatabase() {
            DatabaseInitializer.ReadFromJson();

            DatabaseInitializer.Init();
        }

        private static void InitializeApi() {
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options => {
                options.AddPolicy(name: "AllowSpecificOrigin",
                        policy => {
                            policy.WithOrigins("http://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                        });
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new LoggerProvider());

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseWebSockets();
            app.Use(async (HttpContext context, Func<Task> next) => {
                if (context.Request.Path == "/ws/AddNewLegoSet") {
                    if (context.WebSockets.IsWebSocketRequest) {
                        using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync()) {
                            MyLogger.Log.Information("Connected new client to web socket");
                            var addNewLogSetWebSocket = new AddLegoSetWebSocket();

                            await addNewLogSetWebSocket.Run(webSocket);
                        }
                    } else {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    }
                } else {
                    await next();
                }
            });

            app.UseAuthorization();

            app.MapControllers();

            app.UseCors("AllowSpecificOrigin");

            app.Run();
        }
    }
}
