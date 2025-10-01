using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoApi {
    public class LegoApiFactory {
        internal static string Token { get; set; }

        public static ILegoApi Create() {
            return new LegoApi(Token);
        }
    }
}
