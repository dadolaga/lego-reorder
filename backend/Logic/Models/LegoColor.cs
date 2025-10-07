using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Models {
    public class LegoColor {
        public int? DatabaseId { get; set; }
        public string ApiId { get; set; }
        public string? Name { get; set; }
        public int? Value { get; set; }
        public bool Trasparent { get; set; }
    }
}
