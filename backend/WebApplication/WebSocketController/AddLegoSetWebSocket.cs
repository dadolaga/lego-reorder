using Database;
using Database.Model;
using LegoApi.Models;
using Logic;
using Logic.Models;
using Microsoft.EntityFrameworkCore;
using System.CodeDom.Compiler;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace WebApplication.WebSocketController {
    public class AddLegoSetWebSocket : BaseWebSocket {

        protected override async Task RunImplemented(string uuid, WebSocket webSocket) {
            string received = await ReceiveAsync(webSocket, CancellationToken.None);
            MyLogger.Log.Debug($"Received from {uuid}: {received}");

            var baseData = JsonSerializer.Deserialize<WebApplication.Models.BaseWebSocket<JsonElement>>(received);
            if (baseData!.Code == 1) {
                using var database = new LegoDbContext();
                var legoApi = LegoApi.LegoApiFactory.Create();

                Logic.Models.LegoSet sendedLegoSet = baseData.Data.Deserialize<Logic.Models.LegoSet>()!;

                var transaction = database.Database.BeginTransaction();

                try {
                    var theme = database.Theme.FirstOrDefault(t => t.ApiId == sendedLegoSet.Theme!.ApiId);

                    if(theme == null && sendedLegoSet.Theme != null) {
                        theme = new LegoThemeDb {
                            ApiId = sendedLegoSet.Theme.ApiId!,
                            Name = sendedLegoSet.Theme.Name!,
                        };

                        database.Add(theme);
                        database.SaveChanges();
                    }

                    var dbLegoSet = new LegoSetDb {
                        ApiId = sendedLegoSet.ApiId,
                        LegoCode = sendedLegoSet.LegoCode,
                        Name = sendedLegoSet.Name,
                        Url = sendedLegoSet.ImageUrl,
                        Year = sendedLegoSet.Year,
                        ThemeId = theme?.Id,
                    };

                    try {
                        await database.Sets.AddAsync(dbLegoSet);
                        await database.SaveChangesAsync();
                    } catch (DbUpdateException ex) {
                        MyLogger.Log.Warning($"Try to insert a duplicate lego: {dbLegoSet.LegoCode}", ex.InnerException);

                        await transaction.RollbackAsync();

                        await SendAsync(webSocket, new Models.BaseWebSocket {
                            Code = 11,
                            Message = $"Lego set {dbLegoSet.LegoCode} already exist",
                            Data = null!
                        });

                        return;
                    }

                    await SendAsync(webSocket, new Models.BaseWebSocket {
                        Code = 0,
                        Message = $"Success insert lego set {dbLegoSet.LegoCode}-{dbLegoSet.Name}",
                        Data = null!
                    });

                    var apiPieces = await legoApi.GetAllPieceFromSet(sendedLegoSet.ApiId!);

                    foreach (var piece in apiPieces) {
                        // Invalid if lego api not sended a valid color
                        if (piece.Color == null) {
                            await SendAsync(webSocket, new Models.BaseWebSocket {
                                Code = 10,
                                Message = $"Lego piece {piece.ApiId} not have a lego color",
                                Data = null!
                            });

                            return;
                        }

                        // Request to user to show a correct lego ID
                        if (piece.LegoId == null) {
                            MyLogger.Log.Warning($"Lego id not founder for lego api id: {piece.ApiId}");

                            await SendAsync(webSocket, new Models.BaseWebSocket<List<LegoPiece>> {
                                Code = 13,
                                Message = $"Lego code not found: {piece.Name}",
                                Data = new List<LegoPiece> { piece }
                            });

                            var result = await ReceiveAsync(webSocket, CancellationToken.None);
                            var parsedResult = JsonSerializer.Deserialize<Models.BaseWebSocket<List<LegoPiece>>>(result);

                            if (parsedResult != null && parsedResult.Data.Count() == 1) {
                                var pieceFromFrontend = parsedResult.Data.First();

                                if(pieceFromFrontend.LegoId == null) {
                                    piece.LegoId = GenerateUniqeNullLegoId(database);
                                } else {
                                    piece.LegoId = pieceFromFrontend.LegoId;
                                }
                            }
                        }

                        LegoColorDb dbLegoColor = await AddColorToDb(database, piece);

                        LegoPieceDb dbLegoPiece = await AddPieceToDb(database, piece, webSocket, dbLegoColor);

                        await AddSetPieceToDb(database, piece, dbLegoSet, dbLegoPiece);
                    }

                    transaction.Commit();
                } catch (Exception ex) {
                    transaction.Rollback();

                    DbUpdateException? updateEx = ex as DbUpdateException;
                    if(updateEx != null) {
                        await SendAsync(webSocket, new Models.BaseWebSocket {
                            Code = 10,
                            Message = $"Expection: when try to insert pieces {updateEx.InnerException?.Message}",
                            Data = null!
                        });

                        return;
                    }

                    await SendAsync(webSocket, new Models.BaseWebSocket {
                        Code = 10,
                        Message = $"Expection: when try to insert pieces {ex.Message}",
                        Data = null!
                    });
                }
            }
        }

        private async Task<LegoColorDb> AddColorToDb(LegoDbContext database, LegoPiece piece) {
            LegoColorDb? dbLegoColor = database.Colors.FirstOrDefault(l => l.ApiId == piece.Color.ApiId);

            if (dbLegoColor == null) {
                dbLegoColor = new LegoColorDb {
                    ApiId = piece.Color.ApiId,
                    Name = piece.Color.Name,
                    Value = piece.Color.Value
                };

                await database.AddAsync(dbLegoColor);
                await database.SaveChangesAsync();
            }

            return dbLegoColor;
        }

        private async Task<LegoPieceDb> AddPieceToDb(LegoDbContext database, LegoPiece piece, WebSocket webSocket, LegoColorDb legoColorDb) {
            var dbLegoPiece = database.Pieces.FirstOrDefault(p => p.ApiId == piece.ApiId || (piece.Color != null && p.LegoId == piece.LegoId && p.Color.ApiId == piece.Color.ApiId));

            if (dbLegoPiece == null) {
                try {
                    dbLegoPiece = new LegoPieceDb {
                        ApiId = piece.ApiId,
                        LegoId = piece.LegoId,
                        Name = piece.Name,
                        ImageUrl = piece.ImageUrl,
                        ColorId = legoColorDb.Id
                    };

                    await database.AddAsync(dbLegoPiece);
                    await database.SaveChangesAsync();
                } catch (DbUpdateException ex) {
                    database.ChangeTracker.Clear();

                    var duplicateLegoPiece = database.Pieces.First(p => p.LegoId == piece.LegoId);

                    await SendAsync(webSocket, new Models.BaseWebSocket<List<LegoPiece>> {
                        Code = 13,
                        Message = $"Duplicate lego code: {duplicateLegoPiece.LegoId}",
                        Data = new List<LegoPiece> { piece, duplicateLegoPiece.Convert() }
                    });

                    var result = await ReceiveAsync(webSocket, CancellationToken.None);
                    var parsedResult = JsonSerializer.Deserialize<Models.BaseWebSocket<List<LegoPiece>>>(result);

                    foreach (var pieceRecived in parsedResult!.Data) {
                        LegoPieceDb pieceToUpdate = null!;

                        if (pieceRecived.DatabaseId == null) {
                            pieceToUpdate = new LegoPieceDb {
                                ApiId = pieceRecived.ApiId,
                                LegoId = pieceRecived.LegoId,
                                Name = pieceRecived.Name,
                                ColorId = legoColorDb.Id,
                                ImageUrl = pieceRecived.ImageUrl,
                            };

                            database.Add(pieceToUpdate);
                        } else {
                            pieceToUpdate = database.Pieces.First(p => p.Id == pieceRecived.DatabaseId);

                            pieceToUpdate.LegoId = pieceRecived.LegoId;

                            // THis kill web socker
                            database.Update(pieceToUpdate);
                        }

                        if (pieceRecived.ApiId == piece.ApiId)
                            dbLegoPiece = pieceToUpdate;
                    }
                    var entry = database.Pieces.FirstOrDefault(p => p.ApiId == piece.ApiId);

                    database.SaveChanges();
                }
            }

            return dbLegoPiece;
        }

        private async Task AddSetPieceToDb(LegoDbContext database, LegoPiece piece, LegoSetDb legoSetDb, LegoPieceDb legoPieceDb) {
            var dbSetPiece = database.SetPieces.FirstOrDefault(sp => sp.SetId == legoSetDb.Id && sp.PieceId == legoPieceDb.Id);

            if (dbSetPiece == null) {
                dbSetPiece = new LegoSetPieceDb {
                    SetId = legoSetDb.Id,
                    PieceId = legoPieceDb.Id,
                    Quantity = !piece.isSpare ? piece.Quantity : 0,
                    QuantityHave = 0,
                    SpareQuantity = piece.isSpare ? piece.Quantity : 0
                };

                database.Add(dbSetPiece);
                database.SaveChanges();
            } else if ((!piece.isSpare && dbSetPiece.Quantity == 0) || (piece.isSpare && dbSetPiece.SpareQuantity == 0)) {
                if (piece.isSpare)
                    dbSetPiece.SpareQuantity = piece.Quantity;
                else
                    dbSetPiece.Quantity = piece.Quantity;

                database.Update(dbSetPiece);
                database.SaveChanges();
            } else {
                MyLogger.Log.Error($"Unexpected condition, when try to insert piece {piece.ApiId} on set {legoSetDb.LegoCode}");
            }
        }

        private string GenerateUniqeNullLegoId(LegoDbContext database) {
            const string NullPrefix = "empty-";

            int? lastId = database.Pieces
                .Where(p => p.LegoId.StartsWith(NullPrefix))
                .Select(p => p.LegoId)
                .ToList()
                .Select(legoId => Int32.Parse(legoId.Split("-")[1]))
                .OrderByDescending(id => id)
                .FirstOrDefault();

            if(lastId == null)
                lastId = 0;

            return $"{NullPrefix}{++lastId}";
        }
    }
}
