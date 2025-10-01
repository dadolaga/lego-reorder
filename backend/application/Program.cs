
using LegoApi;

namespace application {
    internal class Program {
        static void Main(string[] args) {
            MyLogger.Log.Init();

            LegoInitializer.Init();

            var api = LegoApiFactory.Create();

            var ciao = api.SearchLegoSetFromCode("4997").Result;

            foreach (var set in ciao)
            {
                Console.WriteLine($"{set.LegoCode} -> {set.Name}");
            }

        }
    }
}
