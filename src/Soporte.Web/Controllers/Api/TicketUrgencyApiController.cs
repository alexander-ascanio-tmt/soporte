using Conexiones.Web.Models.Api;
using Conexiones.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Conexiones.Web.Controllers.Api;

[ApiController]
[AllowAnonymous]
[EnableCors("ExtensionApi")]
[Route("api/ticket-urgency")]
public class TicketUrgencyApiController(
    ITicketUrgencyService urgencyService,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("{ticketNumber}")]
    public async Task<ActionResult<TicketUrgencyResponse>> Get(
        string ticketNumber,
        [FromQuery] string? token,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(token))
        {
            return Unauthorized(new { ok = false, error = "Token inválido." });
        }

        if (string.IsNullOrWhiteSpace(ticketNumber))
        {
            return BadRequest(new { ok = false, error = "Número de ticket requerido." });
        }

        var result = await urgencyService.GetAsync(ticketNumber, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<TicketUrgencyResponse>> Set(
        [FromBody] SetTicketUrgencyRequest request,
        [FromQuery] string? token,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(token))
        {
            return Unauthorized(new { ok = false, error = "Token inválido." });
        }

        if (string.IsNullOrWhiteSpace(request.TicketNumber))
        {
            return BadRequest(new { ok = false, error = "ticketNumber requerido." });
        }

        var result = await urgencyService.SetAsync(
            request.TicketNumber,
            request.IsUrgent,
            updatedByUserName: "Extensión TeamSupport",
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("batch")]
    public async Task<ActionResult<TicketUrgencyBatchResponse>> GetBatch(
        [FromBody] TicketUrgencyBatchRequest request,
        [FromQuery] string? token,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(token))
        {
            return Unauthorized(new { ok = false, error = "Token inválido." });
        }

        if (request.TicketNumbers is null || request.TicketNumbers.Count == 0)
        {
            return BadRequest(new { ok = false, error = "Falta ticketNumbers[] en el body." });
        }

        var result = await urgencyService.GetBatchAsync(request.TicketNumbers, cancellationToken);
        return Ok(result);
    }

    private bool IsAuthorized(string? token)
    {
        var expected = configuration["ExtensionApi:Token"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        if (string.Equals(expected, token, StringComparison.Ordinal))
        {
            return true;
        }

        if (Request.Headers.TryGetValue("X-Api-Key", out var apiKey)
            && string.Equals(expected, apiKey.ToString(), StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
