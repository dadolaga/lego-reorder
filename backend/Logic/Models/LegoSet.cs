using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Logic.Models {
    public class LegoSet {

        [JsonPropertyName("databaseId")]
        public int? DatabaseId { get; set; }

        [JsonPropertyName("apiId")]
        public string? ApiId { get; set; }

        [JsonPropertyName("legoCode")]
        public string? LegoCode { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }
    }
}
