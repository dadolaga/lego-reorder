using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization; // Import necessario

namespace Logic.Models {
    public class LegoPiece {

        [JsonPropertyName("databaseId")]
        public int? DatabaseId { get; set; }

        [JsonPropertyName("legoId")]
        public string? LegoId { get; set; }

        [JsonPropertyName("apiId")]
        public string? ApiId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("color")]
        public LegoColor? Color { get; set; }

        [JsonPropertyName("quantity")]
        public uint Quantity { get; set; }

        [JsonPropertyName("quantitySpare")]
        public uint QuantitySpare { get; set; }

        [JsonPropertyName("quantityHave")]
        public uint? QuantityHave { get; set; }

        [JsonPropertyName("legoSets")]
        public ICollection<LegoSet> LegoSets { get; set; }
    }
}