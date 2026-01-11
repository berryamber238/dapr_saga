using Dapr;
using Microsoft.AspNetCore.Mvc;
using Service.Notification.Services;

namespace Service.Notification.Controllers;

[ApiController]
public class NotificationController : ControllerBase
{
    private readonly WebSocketConnectionManager _wsManager;

    public NotificationController(WebSocketConnectionManager wsManager)
    {
        _wsManager = wsManager;
    }

    [Topic("pubsub", "saga-status")]
    [HttpPost("notify")]
    public async Task<IActionResult> HandleSagaEvent([FromBody] object evt)
    {
        Console.WriteLine($"[Notification] Received Kafka event, broadcasting...");
        await _wsManager.BroadcastAsync(evt);
        return Ok();
    }
}
