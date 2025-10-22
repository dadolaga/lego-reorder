using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model {
    public class LegoSetPiece {
        public int SetId { get; set; }
        public LegoSet Set { get; set; }
        public int PieceId { get; set; }
        public LegoPiece Piece { get; set; }
        public int Quantity { get; set; }
        public int QuantityHave { get; set; }
    }
}
