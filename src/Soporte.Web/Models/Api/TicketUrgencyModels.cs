using System.Text.Json.Serialization;

namespace Soporte.Web.Models.Api;

public sealed class TicketUrgencyResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; } = true;

    [JsonPropertyName("ticketNumber")]
    public string TicketNumber { get; init; } = string.Empty;

    [JsonPropertyName("isUrgent")]
    public bool IsUrgent { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; init; }
}

public sealed class SetTicketUrgencyRequest
{
    [JsonPropertyName("ticketNumber")]
    public string? TicketNumber { get; init; }

    [JsonPropertyName("isUrgent")]
    public bool IsUrgent { get; init; }
}

public sealed class TicketUrgencyBatchRequest
{
    [JsonPropertyName("ticketNumbers")]
    public List<string> TicketNumbers { get; init; } = [];
}

public sealed class TicketUrgencyBatchResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; } = true;

    [JsonPropertyName("matches")]
    public Dictionary<string, TicketUrgencyResponse> Matches { get; init; } = new(StringComparer.Ordinal);
}
