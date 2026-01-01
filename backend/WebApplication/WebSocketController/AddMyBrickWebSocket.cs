using Logic.Models;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Data;
using System.Net.WebSockets;
using System.Text.Json;

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

        private static async Task SendToAll<T>(int legoSetId, Models.BaseWebSocket<T> message) {
            var webSocketList = clients.GetAllWebSocket(legoSetId);

            if (webSocketList != null) {
                foreach (var webSocket in webSocketList) {
                    await SendAsync(webSocket, message);
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

            while (true) {
                var message = await ReceiveAsync<JsonElement>(webSocket, CancellationToken.None);

                if (message.Code == 10) {
                    clients.AssignLegoPiece(uuid, message.Data.GetInt32(), true);

                    await SendActivePieces(legoSet.DatabaseId.Value);
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
}
