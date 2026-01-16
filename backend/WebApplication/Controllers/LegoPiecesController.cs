using Database;
using Database.Model;
using Logic;
using Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication.Models;

namespace WebApplication.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class LegoPiecesController : BaseController {
        [HttpGet("set/{id}")]
        public async Task<IActionResult> GetPieceFromSetId(int id, [FromQuery] FilterOptions filterOptions) {
            using var database = new LegoDbContext();

            var pieces = (await database.SetPieces
                .Where(sp => sp.SetId == id)
                .Include(sp => sp.Piece)
                .ThenInclude(s => s.Color)
                .ToListAsync())
                .ApplyFilter(filterOptions, RetrieveSortPropriety)
                .ToList();

            var quantity = (await database.SetPieces
                .Where(sp => sp.SetId == id)
                .Include(sp => sp.Piece)
                .ThenInclude(s => s.Color)
                .ToListAsync())
                .ApplyFilterForCount(filterOptions, RetrieveSortPropriety)
                .Count();

            var convertedPieces = pieces.Select(p => new LegoPiece {
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
                Quantity = p.Quantity,
                QuantitySpare = p.SpareQuantity,
                QuantityHave = p.QuantityHave
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
