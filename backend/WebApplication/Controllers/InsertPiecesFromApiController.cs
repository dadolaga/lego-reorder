using Database;
using Database.Model;
using Logic.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Diagnostics;
using System.IO.Pipelines;
using WebApplication.Models;

namespace WebApplication.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class InsertPiecesFromApiController : BaseController {
        [HttpPost(Name = "addPieceFromLegoSet")]
        public async Task<IActionResult> AddPiece([FromBody] LegoSet legoSet) {
            using var database = new LegoDbContext();
            using var transaction = database.Database.BeginTransaction();
            var legoAPI = LegoApi.LegoApiFactory.Create();

            try {
                int? databaseSetId = legoSet.DatabaseId.HasValue ? legoSet.DatabaseId.Value : database.Sets.FirstOrDefault(s => s.ApiId == legoSet.ApiId)?.Id;

                if (!databaseSetId.HasValue)
                    return CreateFailResponse(ELEMENT_NOT_FOUND, "Lego set not found on DB");

                var apiPieces = await legoAPI.GetAllPieceFromSet(legoSet.ApiId!);

                var apiPieceColor = apiPieces.Select(p => p.Color).DistinctBy(c => c.ApiId);

                var existingColor = database.Colors;
                var existingPiece = database.Pieces;

                var dbColorToAdd = apiPieceColor.Where(c => existingColor.FirstOrDefault(ec => c.ApiId == ec.ApiId) == null).Select(c => new LegoColorDb {
                    ApiId = c.ApiId,
                    Name = c.Name,
                    Value = c.Value
                });

                database.AddRange(dbColorToAdd);
                await database.SaveChangesAsync();

                var dbLegoPieceToAdd = apiPieces.DistinctBy(p => p.LegoId).Where(p => existingPiece.FirstOrDefault(ep => p.ApiId == ep.ApiId) == null).Select(p => new LegoPieceDb {
                    ApiId = p.ApiId,
                    LegoId = p.LegoId,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    ColorId = database.Colors.First(c => c.ApiId == p.Color.ApiId).Id,
                });

                database.AddRange(dbLegoPieceToAdd);
                await database.SaveChangesAsync();

                var piecesDatabaseIdAndLegoCode = database.Pieces.Select(p => new {p.Id, p.LegoId});

                var dbSetPiece = apiPieces.GroupBy(p => p.LegoId).Select(p => {
                    if(p.Count() == 1) {
                        var piece = p.First();

                        return new LegoSetPieceDb {
                            PieceId = piecesDatabaseIdAndLegoCode.First(p1=> piece.LegoId == p1.LegoId).Id,
                            SetId = databaseSetId.Value,
                            Quantity = piece.Quantity,
                            SpareQuantity = 0,
                            QuantityHave = 0
                        };
                    } else if (p.Count() == 2) {
                        return new LegoSetPieceDb {
                            PieceId = piecesDatabaseIdAndLegoCode.First(p1 => p.Key == p1.LegoId).Id,
                            SetId = databaseSetId.Value,
                            Quantity = p.First(p1 => !p1.isSpare).Quantity,
                            SpareQuantity = p.First(p1 => p1.isSpare).Quantity,
                            QuantityHave = 0
                        };
                    } else {
                        throw new IndexOutOfRangeException($"Expected 1 or 2 lego code duplicate but recived {p.Count()}");
                    }
                });

                database.AddRange(dbSetPiece);
                await database.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreateSuccessEmptyResponse();
            } catch (MySqlException ex) {
                MyLogger.Log.Error("Insert fail", ex);

                await transaction.RollbackAsync();

                return CreateFailResponse(INSERT_DB_ERROR, $"Fail when try to insert new lego set: {ex.InnerException?.Message}");
            }
        }
    }
}
