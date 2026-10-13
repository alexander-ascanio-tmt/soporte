using Soporte.Web.Models.Api;

namespace Soporte.Web.Services;

public interface IClientConnectionLookupService
{
    Task<ClientConnectionLookupResult> LookupAsync(string customerName, CancellationToken cancellationToken = default);
    Task<ClientConnectionBatchLookupResponse> LookupBatchAsync(IEnumerable<string> customers, CancellationToken cancellationToken = default);
}
