using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Models {
    public class LegoPiece {
        public int? DatabaseId { get; set; }
        public string? LegoId { get; set; }
        public string? ApiId { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public LegoColor? Color { get; set; }
        public uint Quantity { get; set; }
        public bool isSpare { get; set; }
        public ICollection<LegoSet> LegoSets { get; set; }
    }
}
