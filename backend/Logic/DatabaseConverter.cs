using Database.Model;
using Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic {
    public static class DatabaseConverter {
        public static LegoPiece Convert(this LegoPieceDb piece) {
            return new LegoPiece {
                DatabaseId = piece.Id,
                ApiId = piece.ApiId,
                LegoId = piece.LegoId,
                Name = piece.Name,
                ImageUrl = piece.ImageUrl,
                LegoSets = { },
                Color = piece.Color != null ? new LegoColor {
                    DatabaseId = piece.Color.Id,
                    ApiId = piece.Color.ApiId,
                    Name = piece.Color.Name,
                    Value = piece.Color.Value,
                    Trasparent = false
                } : null,
                
            };
        }

        public static LegoColor Convert(this LegoColorDb colorDb) {
            return new LegoColor {
                DatabaseId = colorDb.Id,
                ApiId = colorDb.ApiId,
                Name = colorDb.Name,
                Value = colorDb.Value,
                Trasparent = colorDb.Trasparent,
            };
        }
    }
}
