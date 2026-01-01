using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model {
    public class LegoSetPieceDb {
        public int SetId { get; set; }
        public LegoSetDb Set { get; set; }
        public int PieceId { get; set; }
        public LegoPieceDb Piece { get; set; }
        public uint Quantity { get; set; }
        public uint SpareQuantity { get; set; }
        public uint? QuantityHave { get; set; }
    }
}
