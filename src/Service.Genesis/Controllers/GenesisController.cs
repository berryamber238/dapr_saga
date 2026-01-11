using Microsoft.AspNetCore.Mvc;
using Service.Genesis.Services;

namespace Service.Genesis.Controllers;

public class TransactionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
}

[ApiController]
[Route("api/genesis")]
public class GenesisController : ControllerBase
{
    private readonly IGenesisService _service;

    public GenesisController(IGenesisService service)
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
