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

        foreach (var customer in customers.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            response.Matches[customer] = Resolve(index, customer);
        }

        return response;
    }

    private async Task<Dictionary<string, List<ClientConnection>>> BuildIndexAsync(CancellationToken cancellationToken)
    {
        var connections = await dbContext.ClientConnections
            .AsNoTracking()
            .Include(c => c.ConnectionType)
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

        return index;
    }

    private static IEnumerable<string> GetLookupKeys(ClientConnection connection)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        var nameKey = ClientConnectionNormalizer.Normalize(connection.TeamSupportClientName);
        if (!string.IsNullOrEmpty(nameKey))
        {
            keys.Add(nameKey);
        }

        var razonKey = ClientConnectionNormalizer.Normalize(connection.TeamSupportClientRazonSocial);
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
        var key = ClientConnectionNormalizer.Normalize(customerName);

        if (string.IsNullOrEmpty(key) || !index.TryGetValue(key, out var matches) || matches.Count == 0)
        {
            return new ClientConnectionLookupResult
            {
                Ok = false,
                Match = null,
                Conexion = null,
                Observaciones = null
            };
        }

        return ToResult(matches);
    }

    private static ClientConnectionLookupResult ToResult(IReadOnlyList<ClientConnection> matches)
    {
        var primary = matches[0];
        var matchLabel = string.IsNullOrWhiteSpace(primary.TeamSupportClientRazonSocial)
            ? primary.TeamSupportClientName
            : primary.TeamSupportClientRazonSocial;

        var conexion = string.Join(
            ", ",
            matches
                .Select(c => c.ConnectionType.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase));

        var observaciones = string.Join(
            "\n",
            matches
                .Select(c => c.Notes)
                .Where(note => !string.IsNullOrWhiteSpace(note))
                .Distinct(StringComparer.OrdinalIgnoreCase));

        return new ClientConnectionLookupResult
        {
            Ok = true,
            Match = matchLabel,
            Conexion = string.IsNullOrWhiteSpace(conexion) ? null : conexion,
            Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones
        };
    }
}
