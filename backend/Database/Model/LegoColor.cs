using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model {
    internal class LegoColor {
        public int Id { get; set; }
        public string ApiId { get; set; }
        public string? Name { get; set; }
        public int? Value { get; set; }
        public ICollection<LegoPiece> Pieces { get; set; }
    }
}
