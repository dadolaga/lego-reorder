using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model {
    public class LegoSetDb {
        public int Id { get; set; }
        public string ApiId { get; set; }
        public string LegoCode { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public string? Url { get; set; }
        public ICollection<LegoSetPieceDb> Pieces { get; set; }
    }
}
