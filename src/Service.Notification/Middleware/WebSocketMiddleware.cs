using System.Net.WebSockets;
using System.Text;

namespace Service.Notification.Middleware;

using Service.Notification.Services;

public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly WebSocketConnectionManager _manager;

    public WebSocketMiddleware(RequestDelegate next, WebSocketConnectionManager manager)
    {
        _next = next;
        _manager = manager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = _manager.AddSocket(webSocket);
            
            Console.WriteLine($"[WebSocket] Client connected: {socketId}");

            try
            {
                await Receive(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"[WebSocket] Received: {message}");
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _manager.RemoveSocketAsync(socketId);
                    }
                });
            }
            catch
            {
                await _manager.RemoveSocketAsync(socketId);
            }
        }
        else
        {
            await _next(context);
        }
    }

    private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            handleMessage(result, buffer);
        }
    }
}
