using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model {
    public class LegoThemeDb {
        public int Id { get; set; }
        public string ApiId { get; set; }
        public string Name { get; set; }
        public ISet<LegoSetDb> Sets { get; set; }
    }
}
