using Database.Model;
using Logic.Models;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Data;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication.WebSocketController {
    public class AddMyBrickWebSocket : BaseWebSocket {
        private static WebSocketList clients = new WebSocketList();

        private LegoSet legoSet;

        private static async Task SendActivePieces(int legoSetId) {
            var data = new Models.BaseWebSocket<List<int>> {
                Code = 10,
                Message = "Selected piece",
                Data = clients.GetAllActivePieces(legoSetId).ToList()
            };

            await SendToAll(legoSetId, data);
        }

        private static async Task SendAllPieceQuantity(int legoSetId) {
            using var database = new Database.LegoDbContext();

            var data = new Models.BaseWebSocket<List<SetPieceQuantityHave>> {
                Code = 20,
                Message = "Quantity for all pieces",
                Data = database.SetPieces
                    .Where(sp => sp.SetId == legoSetId && sp.QuantityHave != null)
                    .Select(sp => new SetPieceQuantityHave { PieceId = sp.PieceId, QuantityHave = (uint)sp.QuantityHave! })
                    .ToList()
            };

            await SendToAll(legoSetId, data);
        }

        private static async Task SendPieceQuantity(int legoSetId, int legoPieceId, uint legoQuantity) {
            var data = new Models.BaseWebSocket<SetPieceQuantityHave> {
                Code = 21,
                Message = "Quantity for single pieces",
                Data = new SetPieceQuantityHave {
                    PieceId = legoPieceId,
                    QuantityHave = legoQuantity
                }
            };

            await SendToAll(legoSetId, data);
        }

        private static async Task SendToAll<T>(int legoSetId, Models.BaseWebSocket<T> message) {
            var webSocketList = clients.GetAllWebSocket(legoSetId);

            if (webSocketList != null) {
                foreach (var webSocket in webSocketList) {
                    if (webSocket.State == WebSocketState.Open) {
                        await SendAsync(webSocket, message);
                    } else {
                        MyLogger.Log.Warning("Web socket is not in correct state. Removing...");
                    }
                }
            }
        }

        protected override async Task RunImplemented(string uuid, WebSocket webSocket) {
            clients.AddNewClient(uuid, webSocket);

            var legoSetMessage = await ReceiveAsync<LegoSet>(webSocket, CancellationToken.None);

            legoSet = legoSetMessage.Data;

            if (legoSet.DatabaseId == null) {
                throw new ArgumentNullException("Lego set id not recevied");
            }

            clients.AssignLegoSet(uuid, legoSet.DatabaseId.Value, true);

            await SendActivePieces(legoSet.DatabaseId.Value);

            await SendAllPieceQuantity(legoSet.DatabaseId.Value);

            while (true) {
                var message = await ReceiveAsync<JsonElement>(webSocket, CancellationToken.None);

                if (message.Code == 10) {
                    clients.AssignLegoPiece(uuid, message.Data.GetInt32(), true);

                    await SendActivePieces(legoSet.DatabaseId.Value);
                } else if (message.Code == 11) {
                    var pieceId = message.Data.GetProperty("pieceId").GetInt32();
                    var quantity = message.Data.GetProperty("quantity").GetUInt32();

                    using (var database = new Database.LegoDbContext()) {
                        var setPiece = database.SetPieces.First(sp => sp.SetId == legoSet.DatabaseId.Value && sp.PieceId == pieceId);

                        setPiece.QuantityHave = quantity;

                        database.Update(setPiece);
                        await database.SaveChangesAsync();
                    }

                    await SendPieceQuantity(legoSet.DatabaseId.Value, pieceId, quantity);
                } else {
                    MyLogger.Log.Warning($"Received message not recognized: {message.Message} ({message.Code})");
                }
            }
        }

        protected override async Task ClearImplemented(string uuid, WebSocket webSocket) {
            clients.RemoveClient(uuid);

            if (legoSet != null && legoSet.DatabaseId != null) {
                await SendActivePieces(legoSet.DatabaseId.Value);
            }
        }
    }

    internal class WebSocketList {
        private LinkedList<Client> clients;

        public WebSocketList() {
            clients = new LinkedList<Client>();
        }

        public void AddNewClient(string uuid, WebSocket webSocket) {
            lock (clients) {
                if (clients.FirstOrDefault(c => c.Uuid == uuid) != null) {
                    throw new DuplicateNameException("Duplicate UUID");
                }

                clients.AddLast(new Client {
                    Uuid = uuid,
                    WebSocket = webSocket
                });
            }
        }

        public void RemoveClient(string uuid) {
            lock (clients) {
                var client = clients.FirstOrDefault(c => c.Uuid == uuid);

                if (client == null) {
                    throw new KeyNotFoundException("UUID not found");
                }

                clients.Remove(client);
            }
        }

        public void RemovingAllClosedOrError() {
            lock (clients) {
                foreach (var client in clients) {
                    if (client.WebSocket.State == WebSocketState.Aborted
                        || client.WebSocket.State == WebSocketState.CloseReceived
                        || client.WebSocket.State == WebSocketState.CloseSent) {
                        MyLogger.Log.Warning($"Removing web socket {client.Uuid}");

                        clients.Remove(client);
                    }
                }
            }
        }

        public void AssignLegoSet(string uuid, int legoSetId, bool force = false) {
            lock (clients) {
                var client = clients.FirstOrDefault(c => c.Uuid == uuid);

                if (client == null) {
                    throw new KeyNotFoundException("UUID not found");
                }

                if (!force && client.ActiveSetId != null) {
                    throw new Exception("Lego set alredy inserted, and not forced");
                }

                client.ActiveSetId = legoSetId;
            }
        }

        public void AssignLegoPiece(string uuid, int legoPieceId, bool force = false) {
            lock (clients) {
                var client = clients.FirstOrDefault(c => c.Uuid == uuid);

                if (client == null) {
                    throw new KeyNotFoundException("UUID not found");
                }

                if (!force && client.ActiveSetId != null) {
                    throw new Exception("Lego piece alredy inserted, and not forced");
                }

                client.ActivePieceId = legoPieceId;
            }
        }

        public IEnumerable<WebSocket> GetAllWebSocket(int? legoSetIdFilter = null) {
            lock (clients) {
                return clients
                    .Where(c => (legoSetIdFilter != null ? (c.ActiveSetId == legoSetIdFilter) : true))
                    .Select(c => c.WebSocket);
            }
        }

        public IEnumerable<int> GetAllActivePieces(int legoSetIdFilter) {
            lock (clients) {
                return clients
                    .Where(c => c.ActiveSetId == legoSetIdFilter && c.ActivePieceId != null)
                    .Select(c => c.ActivePieceId!.Value);
            }
        }

        private class Client {
            public string Uuid { get; set; }
            public WebSocket WebSocket { get; set; }
            public int? ActiveSetId { get; set; }
            public int? ActivePieceId { get; set; }
        }
    }

    internal class SetPieceQuantityHave {
        [JsonPropertyName("pieceId")]
        public int PieceId { get; set; }
        [JsonPropertyName("quantityHave")]
        public uint QuantityHave { get; set; }
    }
}
