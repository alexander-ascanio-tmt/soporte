using System.Text.Json.Serialization;

namespace Conexiones.Web.Models.Api;

public sealed class ClientConnectionLookupResult
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("match")]
    public string? Match { get; init; }

    [JsonPropertyName("Conexion")]
    public string Conexion { get; init; } = string.Empty;

    [JsonPropertyName("Observaciones")]
    public string Observaciones { get; init; } = string.Empty;

    [JsonPropertyName("Contacto")]
    public string Contacto { get; init; } = string.Empty;

    [JsonPropertyName("Telefono")]
    public string Telefono { get; init; } = string.Empty;

    [JsonPropertyName("Email")]
    public string Email { get; init; } = string.Empty;

    public static ClientConnectionLookupResult NotFound() => new()
    {
        Ok = false,
        Match = null
    };
}

public sealed class ClientConnectionBatchLookupRequest
{
    [JsonPropertyName("customers")]
    public List<string> Customers { get; init; } = [];
}

public sealed class ClientConnectionBatchLookupResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; } = true;

    [JsonPropertyName("matches")]
    public Dictionary<string, ClientConnectionLookupResult> Matches { get; init; } = new(StringComparer.Ordinal);
}
