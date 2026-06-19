using System.Text.Json.Serialization;

namespace Conexiones.Web.Models.Api;

public sealed class ClientConnectionLookupResult
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("match")]
    public string? Match { get; init; }

    [JsonPropertyName("Conexion")]
    public string? Conexion { get; init; }

    [JsonPropertyName("Observaciones")]
    public string? Observaciones { get; init; }
}

public sealed class ClientConnectionBatchLookupRequest
{
    public List<string> Customers { get; init; } = [];
}

public sealed class ClientConnectionBatchLookupResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; } = true;

    [JsonPropertyName("matches")]
    public Dictionary<string, ClientConnectionLookupResult> Matches { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
