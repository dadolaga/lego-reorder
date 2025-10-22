using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model {
    public class LegoPiece {
        public int Id { get; set; }
        public string LegoId { get; set; }
        public string ApiId { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public ushort ColorId { get; set; }
        public LegoColor Color { get; set; }
        public ICollection<LegoSetPiece> Sets { get; set; }
    }
}
