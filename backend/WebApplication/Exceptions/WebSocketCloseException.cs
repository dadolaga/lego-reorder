using System.Net.WebSockets;
using System.Runtime.Serialization;

namespace WebApplication.Exceptions {
    public class WebSocketCloseException : Exception {
        WebSocketCloseStatus Status { get; init; }

        public WebSocketCloseException(WebSocketCloseStatus status) {
            Status = status;
        }

        public WebSocketCloseException(WebSocketCloseStatus status, string? message) : base(message) {
            Status = status;
        }

        public WebSocketCloseException(WebSocketCloseStatus status, string? message, Exception? innerException) : base(message, innerException) {
            Status = status;
        }

        protected WebSocketCloseException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

    }
}
