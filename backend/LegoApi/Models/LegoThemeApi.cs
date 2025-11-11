using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LegoApi.Models {
    internal class LegoThemeApi {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("parent_id")]
        public int? ParentId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    internal static class LegoThemeApiUtils {
        public static LegoThemeApi GetAbsoluteParent(this IDictionary<int, LegoThemeApi> legoApiDictionary, int legoThemeId) {
            LegoThemeApi? legoTheme = null;

            do {
                legoTheme = legoApiDictionary[legoTheme == null ? legoThemeId : legoTheme.ParentId!.Value];
            } while (legoTheme.ParentId != null);

            return legoTheme;
        }
    }
}
