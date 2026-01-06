namespace WebApplication.Models {
    public class FilterOptions {
        public int Page { get; set; } = 0;
        public int Limit { get; set; } = 100;
        public string? Sort { get; set; }
        public string? Where { get; set; }
    }
}
