using Microsoft.AspNetCore.Mvc;
using Service.Query.Services;

namespace Service.Query.Controllers;

[ApiController]
[Route("api/query")]
public class QueryController : ControllerBase
{
    private readonly IQueryService _service;

    public QueryController(IQueryService service)
    {
        _service = service;
    }

    [HttpGet("transaction/{transactionId}")]
    public async Task<IActionResult> GetTransaction(string transactionId)
    {
        var result = await _service.GetTransactionDetailsAsync(transactionId);
        if (result == null) return NotFound(new { Message = "Transaction not found" });
        return Ok(new { Code = 200, Message = "Success", Data = result });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? status,
        [FromQuery] string? serviceName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.SearchTransactionsAsync(status, serviceName, page, pageSize);
        return Ok(new { Code = 200, Message = "Success", Data = result });
    }
}
