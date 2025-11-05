using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace WebApplication.Models {
    public class BaseWebSocket : BaseWebSocket<object> { }

    public class BaseWebSocket<T> {

        [JsonPropertyName("code")]
        public int Code { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
