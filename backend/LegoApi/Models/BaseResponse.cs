using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoApi.Models {
    internal class BaseResponse<T> {
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previos { get; set; }
        public ICollection<T> Results { get; set; }
    }
}
