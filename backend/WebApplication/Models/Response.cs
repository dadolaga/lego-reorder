using System.Runtime.InteropServices;

namespace WebApplication.Models {
    public class Response : Response<object> { }

    public class Response<T> {
        public int Code { get; set; }
        public string? Message { get; set; }
        public T Data { get; set; }
    }
}
