using System.Runtime.CompilerServices;
using System.Text.Json;

namespace LegoApi {
    public class LegoInitializer {
        public static void Init(string token) {
            LegoApiFactory.Token = token;
        }
    }
}
