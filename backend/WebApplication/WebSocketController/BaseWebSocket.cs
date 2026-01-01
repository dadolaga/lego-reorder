using Logic.Models;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Net.WebSockets;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using WebApplication.Exceptions;

namespace WebApplication.WebSocketController {
    public abstract class BaseWebSocket {
        private static ConcurrentDictionary<string, WebSocket>? clients_ = null;
        protected static ConcurrentDictionary<string, WebSocket> Clients {
            get {
                if (clients_ == null)
                    clients_ = new ConcurrentDictionary<string, WebSocket>();

                return clients_;
            }
        }

        protected static async Task SendAsync(string uuid, string data) {
            await SendAsync(Clients.First(p => p.Key == uuid).Value, Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text);
        }

        protected static async Task SendAsync(string uuid, byte[] data) {
            await SendAsync(Clients.First(p => p.Key == uuid).Value, data, WebSocketMessageType.Binary);
        }

        protected static async Task SendAsync<T>(string uuid, T data) {
            await SendAsync(Clients.First(p => p.Key == uuid).Value, JsonSerializer.SerializeToUtf8Bytes(data, JsonSerializerOptions.Default), WebSocketMessageType.Binary);
        }

        protected static async Task SendAsync(WebSocket webSocket, string data) {
            await SendAsync(webSocket, Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text);
        }

        protected static async Task SendAsync(WebSocket webSocket, byte[] data) {
            await SendAsync(webSocket, data, WebSocketMessageType.Binary);
        }

        protected static async Task SendAsync<T>(WebSocket webSocket, T data) {
            await SendAsync(webSocket, JsonSerializer.SerializeToUtf8Bytes(data, JsonSerializerOptions.Default), WebSocketMessageType.Text);
        }

        private static async Task SendAsync(WebSocket webSocket, byte[] data, WebSocketMessageType messageType) {
            await webSocket.SendAsync(new ArraySegment<byte>(data), messageType, true, CancellationToken.None);
        }

        protected async Task<Models.BaseWebSocket<T>?> ReceiveAsync<T>(string uuid, CancellationToken cancellationToken) {
            var result = await ReceiveAsync(Clients.First(p => p.Key == uuid).Value, cancellationToken);
            return JsonSerializer.Deserialize<Models.BaseWebSocket<T>>(result);
        }

        protected async Task<Models.BaseWebSocket<T>?> ReceiveAsync<T>(WebSocket webSocket, CancellationToken cancellationToken) {
            var result = await ReceiveAsync(webSocket, cancellationToken);
            return JsonSerializer.Deserialize<Models.BaseWebSocket<T>>(result);
        }

        protected async Task<string> ReceiveAsync(string uuid, CancellationToken cancellationToken) {
            return await ReceiveAsync(Clients.First(p => p.Key == uuid).Value, cancellationToken);
        }

        protected async Task<string> ReceiveAsync(WebSocket webSocket, CancellationToken cancellationToken) {
            var buffer = new byte[1024];
            var builder = new StringBuilder();
            WebSocketReceiveResult result;

            do {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.CloseStatus.HasValue) {
                    throw new WebSocketCloseException(result.CloseStatus.Value);
                }

                builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            } while (!result.EndOfMessage);

            return builder.ToString();
        }

        private string AddNewSocket(WebSocket webSocket) {
            string uuid = "";
            bool result = false;

            do {
                uuid = Guid.NewGuid().ToString("N");

                result = Clients.TryAdd(uuid, webSocket);
            } while (!result);

            return uuid;
        }

        public async Task Run(WebSocket webSocket) {
            var uuid = AddNewSocket(webSocket);

            MyLogger.Log.Debug($"New websocket connected: {uuid}");

            await SendAsync(webSocket, uuid);

            try {
                await RunImplemented(uuid, webSocket);
            } catch (WebSocketCloseException ex) {
                MyLogger.Log.Debug($"Close websocket: {uuid}");
            }

            await ClearImplemented(uuid, webSocket);

            Clients.Remove(uuid, out _);
        }

        abstract protected Task RunImplemented(string uuid, WebSocket webSocket);

        virtual protected Task ClearImplemented(string uuid, WebSocket webSocket) {
            return Task.Run(() => {

            });
        }
    }
}
