using System.Text.Json.Serialization;

namespace LegoApi.Models {


    internal class LegoPart {
        public int Id { get; set; }

        [JsonPropertyName("inv_part_id")]
        public int InvPartId { get; set; }

        public Part Part { get; set; }

        public Color Color { get; set; }

        [JsonPropertyName("set_num")]
        public string SetNum { get; set; }

        public int Quantity { get; set; }

        [JsonPropertyName("is_spare")]
        public bool IsSpare { get; set; }

        [JsonPropertyName("element_id")]
        public string ElementId { get; set; }

        [JsonPropertyName("num_sets")]
        public int NumSets { get; set; }
    }

    internal class Part {
        [JsonPropertyName("part_num")]
        public string PartNum { get; set; }

        public string Name { get; set; }

        [JsonPropertyName("part_cat_id")]
        public int PartCatId { get; set; }

        [JsonPropertyName("part_url")]
        public string PartUrl { get; set; }

        [JsonPropertyName("part_img_url")]
        public string PartImgUrl { get; set; }

        [JsonPropertyName("external_ids")]
        public ExternalIds ExternalIds { get; set; }
    }

    internal class Color {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Rgb { get; set; }

        [JsonPropertyName("is_trans")]
        public bool IsTrans { get; set; }

        [JsonPropertyName("external_ids")]
        public ColorExternalIds ExternalIds { get; set; }
    }

    internal class ExternalIds {
        public string[] BrickLink { get; set; }
        public string[] BrickOwl { get; set; }
        public string[] Brickset { get; set; }
        public string[] LDraw { get; set; }
        public string[] LEGO { get; set; }
    }

    internal class ColorExternalIds {
        public ColorExtDetail BrickLink { get; set; }
        public ColorExtDetail BrickOwl { get; set; }
        public ColorExtDetail LEGO { get; set; }
        public ColorExtDetail Peeron { get; set; }
        public ColorExtDetail LDraw { get; set; }
    }

    internal class ColorExtDetail {
        [JsonPropertyName("ext_ids")]
        public int?[] ExtIds { get; set; }

        [JsonPropertyName("ext_descrs")]
        public string[][] ExtDescrs { get; set; }
    }
}
