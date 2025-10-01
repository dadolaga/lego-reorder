using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LegoApi.Models {
    public class LegoSet {
        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("theme_id")]
        public int ThemeId { get; set; }

        [JsonPropertyName("num_parts")]
        public int NumParts { get; set; }

        [JsonPropertyName("set_img_url")]
        public string SetImgUrl { get; set; }

        [JsonPropertyName("set_url")]
        public string SetUrl { get; set; }

        [JsonPropertyName("last_modified_dt")]
        public string LastModifiedDt { get; set; }
    }
}
