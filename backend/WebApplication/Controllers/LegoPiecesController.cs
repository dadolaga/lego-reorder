using Database;
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

            var set = database.Sets.Where(p => p.Id == id).Include(p => p.Pieces).ThenInclude(sp => sp.Piece).ThenInclude(p => p.Color).First();

            var queryable = set.Pieces;

            var quantity = set.Pieces.Count();

            var pieces = queryable.Skip((page) * limit).Take(limit).Select(p => new LegoPiece {
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

            return CreateSuccessListResponse(pieces, quantity);
        }
    }
}
