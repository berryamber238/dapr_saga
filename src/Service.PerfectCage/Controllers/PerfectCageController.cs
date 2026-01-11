using Microsoft.AspNetCore.Mvc;
using Service.PerfectCage.Services;

namespace Service.PerfectCage.Controllers;

public class TransactionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
}

[ApiController]
[Route("api/perfectcage")]
public class PerfectCageController : ControllerBase
{
    private readonly IPerfectCageService _service;

    public PerfectCageController(IPerfectCageService service)
    {
        _service = service;
    }

    [HttpPost("transaction")]
    public async Task<IActionResult> Transaction([FromBody] TransactionRequest request)
    {
        var result = await _service.ProcessTransactionAsync(request.TransactionId, request.BusinessId, request.Payload);
        return result ? Ok() : BadRequest();
    }

    [HttpPost("compensate")]
    public async Task<IActionResult> Compensate([FromBody] TransactionRequest request)
    {
        var result = await _service.CompensateTransactionAsync(request.TransactionId);
        return result ? Ok() : BadRequest();
    }
}
