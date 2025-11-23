using Database;
using Database.Model;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class LegoPiecesController : BaseController {
        [HttpGet("set/{id}")]
        public async Task<IActionResult> GetPieceFromSetId(int id, [FromQuery] int page = 0, [FromQuery] int limit = 1000, [FromQuery] string? sort = null) {
            using var database = new LegoDbContext();

            IEnumerable<LegoSetPieceDb> pieces = await database.SetPieces
                .Where(sp => sp.SetId == id)
                .Include(sp => sp.Piece)
                .ThenInclude(s => s.Color)
                .ToListAsync();

            IOrderedEnumerable<LegoSetPieceDb>? orderPieces = null;

            var quantity = pieces.Count();

            if (sort != null) {
                foreach (var sortItem in sort.Split(",")) {
                    bool isDescent = sortItem.StartsWith("-");
                    string proprietyName = (sortItem.StartsWith("+") || sortItem.StartsWith("-")) ? sortItem.Substring(1) : sortItem;

                    if (orderPieces == null) {
                        if (!isDescent) {
                            orderPieces = pieces.OrderBy(v => RetrieveSortPropriety(v, proprietyName));
                        } else {
                            orderPieces = pieces.OrderByDescending(v => RetrieveSortPropriety(v, proprietyName));
                        }
                    } else {
                        if (!isDescent) {
                            orderPieces = orderPieces.ThenBy(v => RetrieveSortPropriety(v, proprietyName));
                        } else {
                            orderPieces = orderPieces.ThenByDescending(v => RetrieveSortPropriety(v, proprietyName));
                        }
                    }
                }
            } else {
                orderPieces = pieces.OrderBy(sp => sp.PieceId);
            }

            var convertedPieces = orderPieces!.Skip((page) * limit).Take(limit).Select(p => new LegoPiece {
                DatabaseId = p.Piece.Id,
                ApiId = p.Piece.ApiId,
                LegoId = p.Piece.LegoId,
                Name = p.Piece.Name,
                ImageUrl = p.Piece.ImageUrl,
                Color = new LegoColor {
                    DatabaseId = p.Piece.Color.Id,
                    ApiId = p.Piece.Color.ApiId,
                    Name = p.Piece.Color.Name,
                    Value = p.Piece.Color.Value,
                    Trasparent = false
                },
                isSpare = p.SpareQuantity != 0,
                Quantity = p.SpareQuantity != 0 ? p.SpareQuantity : p.Quantity,
            });

            return CreateSuccessListResponse(convertedPieces, quantity);
        }

        private static object? RetrieveSortPropriety(LegoSetPieceDb setPieceDb, string sortColumn) {
            if (sortColumn.Equals("color", StringComparison.InvariantCultureIgnoreCase)) {
                return setPieceDb.Piece.ColorId;
            } else if (sortColumn.Equals("quantity", StringComparison.InvariantCultureIgnoreCase)) {
                return setPieceDb.Quantity;
            }

            return null;
        }
    }
}
