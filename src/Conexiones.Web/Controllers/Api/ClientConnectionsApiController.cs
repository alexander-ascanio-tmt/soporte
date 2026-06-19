using Conexiones.Web.Models.Api;
using Conexiones.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conexiones.Web.Controllers.Api;

[ApiController]
[Route("api/client-connections")]
public class ClientConnectionsApiController(
    IClientConnectionLookupService lookupService,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("lookup")]
    public async Task<ActionResult<ClientConnectionLookupResult>> Lookup(
        [FromQuery] string? q,
        [FromQuery] string? token,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(token))
        {
            return Unauthorized(new { ok = false, error = "Token inválido." });
        }

        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { ok = false, error = "Parámetro q requerido." });
        }

        var result = await lookupService.LookupAsync(q, cancellationToken);
        return Ok(result);
    }

    [HttpPost("lookup/batch")]
    public async Task<ActionResult<ClientConnectionBatchLookupResponse>> LookupBatch(
        [FromBody] ClientConnectionBatchLookupRequest request,
        [FromQuery] string? token,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(token))
        {
            return Unauthorized(new { ok = false, error = "Token inválido." });
        }

        var customers = request.Customers ?? [];
        var result = await lookupService.LookupBatchAsync(customers, cancellationToken);
        return Ok(result);
    }

    private bool IsAuthorized(string? token)
    {
        var expected = configuration["ExtensionApi:Token"];
        return !string.IsNullOrWhiteSpace(expected)
            && string.Equals(expected, token, StringComparison.Ordinal);
    }
}
