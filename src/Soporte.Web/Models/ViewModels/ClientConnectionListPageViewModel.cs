using Soporte.Web.Models.Entities;

namespace Soporte.Web.Models.ViewModels;

public class ClientConnectionListPageViewModel
{
    public IReadOnlyList<ClientConnection> Connections { get; init; } = [];
    public ClientConnectionListQuery Query { get; init; } = new();
    public IReadOnlyList<ConnectionTypeOption> ConnectionTypes { get; init; } = [];
    public IReadOnlyList<string> RegisteredByUsers { get; init; } = [];
    public int TotalCount { get; init; }

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Query.Search)
        || Query.ConnectionTypeId.HasValue
        || !string.IsNullOrWhiteSpace(Query.RegisteredBy)
        || (!string.IsNullOrWhiteSpace(Query.Limit) && Query.Limit is not "100");
}
