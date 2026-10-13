using Conexiones.Web.Data;
using Conexiones.Web.Models.Api;
using Conexiones.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conexiones.Web.Services;

public class ClientConnectionLookupService(ApplicationDbContext dbContext) : IClientConnectionLookupService
{
    public async Task<ClientConnectionLookupResult> LookupAsync(
        string customerName,
        CancellationToken cancellationToken = default)
    {
        var index = await BuildIndexAsync(cancellationToken);
        return Resolve(index, customerName);
    }

    public async Task<ClientConnectionBatchLookupResponse> LookupBatchAsync(
        IEnumerable<string> customers,
        CancellationToken cancellationToken = default)
    {
        var index = await BuildIndexAsync(cancellationToken);
        var response = new ClientConnectionBatchLookupResponse();

        foreach (var customer in customers)
        {
            var trimmed = TrimKey(customer);
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            if (response.Matches.ContainsKey(trimmed))
            {
                continue;
            }

            response.Matches[trimmed] = Resolve(index, trimmed);
        }

        return response;
    }

    private async Task<Dictionary<string, List<ClientConnection>>> BuildIndexAsync(CancellationToken cancellationToken)
    {
        var connections = await dbContext.ClientConnections
            .AsNoTracking()
            .Include(c => c.ConnectionType)
            .OrderByDescending(c => c.RegisteredAt)
            .ToListAsync(cancellationToken);

        var index = new Dictionary<string, List<ClientConnection>>(StringComparer.Ordinal);

        foreach (var connection in connections)
        {
            foreach (var key in GetLookupKeys(connection))
            {
                if (!index.TryGetValue(key, out var bucket))
                {
                    bucket = [];
                    index[key] = bucket;
                }

                if (bucket.All(c => c.Id != connection.Id))
                {
                    bucket.Add(connection);
                }
            }
        }

        foreach (var bucket in index.Values)
        {
            bucket.Sort((a, b) => b.RegisteredAt.CompareTo(a.RegisteredAt));
        }

        return index;
    }

    private static IEnumerable<string> GetLookupKeys(ClientConnection connection)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        var nameKey = TrimKey(connection.TeamSupportClientName);
        if (!string.IsNullOrEmpty(nameKey))
        {
            keys.Add(nameKey);
        }

        var razonKey = TrimKey(connection.TeamSupportClientRazonSocial);
        if (!string.IsNullOrEmpty(razonKey))
        {
            keys.Add(razonKey);
        }

        return keys;
    }

    private static ClientConnectionLookupResult Resolve(
        Dictionary<string, List<ClientConnection>> index,
        string customerName)
    {
        var key = TrimKey(customerName);

        if (string.IsNullOrEmpty(key) || !index.TryGetValue(key, out var matches) || matches.Count == 0)
        {
            return ClientConnectionLookupResult.NotFound();
        }

        return ToResult(matches);
    }

    private static ClientConnectionLookupResult ToResult(IReadOnlyList<ClientConnection> matches)
    {
        var primary = matches[0];
        var matchLabel = !string.IsNullOrWhiteSpace(primary.TeamSupportClientName)
            ? primary.TeamSupportClientName
            : primary.TeamSupportClientRazonSocial ?? string.Empty;

        var conexion = string.Join(
            ", ",
            matches
                .Select(c => c.ConnectionType.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal));

        var observaciones = string.Join(
            "\n",
            matches
                .Select(c => c.Notes)
                .Where(note => !string.IsNullOrWhiteSpace(note))
                .Distinct(StringComparer.Ordinal));

        return new ClientConnectionLookupResult
        {
            Ok = true,
            Match = matchLabel,
            NombreCliente = primary.TeamSupportClientName ?? string.Empty,
            RazonSocial = primary.TeamSupportClientRazonSocial ?? string.Empty,
            Conexion = conexion,
            Observaciones = observaciones,
            Contacto = primary.ConnectionType.Contact ?? string.Empty,
            Telefono = primary.ConnectionType.Phone ?? string.Empty,
            Email = primary.ConnectionType.Email ?? string.Empty
        };
    }

    private static string? TrimKey(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
