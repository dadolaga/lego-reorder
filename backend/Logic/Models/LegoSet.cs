using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Models {
    public class LegoSet {
        public int? DatabaseId { get; set; }
        public string? ApiId { get; set; }
        public string LegoCode { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public string? ImageUrl { get; set; }
    }
}
