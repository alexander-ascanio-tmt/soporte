using Soporte.Web.Models.Api;
using Soporte.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Soporte.Web.Controllers.Api;

[ApiController]
[AllowAnonymous]
[EnableCors("ExtensionApi")]
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

        if (request.Customers is null || request.Customers.Count == 0)
        {
            return BadRequest(new { ok = false, error = "Falta customers[] en el body." });
        }

        var result = await lookupService.LookupBatchAsync(request.Customers, cancellationToken);
        return Ok(result);
    }

    private bool IsAuthorized(string? token)
    {
        var expected = configuration["ExtensionApi:Token"];
        return !string.IsNullOrWhiteSpace(expected)
            && string.Equals(expected, token, StringComparison.Ordinal);
    }
}
