namespace Soporte.Web.Models.ViewModels;

public class UrgentTicketListPageViewModel
{
    public IReadOnlyList<UrgentTicketListItemViewModel> Tickets { get; init; } = [];
    public UrgentTicketListQuery Query { get; init; } = new();
    public IReadOnlyList<string> Customers { get; init; } = [];
    public IReadOnlyList<string> Products { get; init; } = [];
    public IReadOnlyList<string> Statuses { get; init; } = [];
    public IReadOnlyList<string> Priorities { get; init; } = [];
    public int TotalCount { get; init; }
    public int VisibleCount { get; init; }

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Query.Search)
        || !string.IsNullOrWhiteSpace(Query.Customer)
        || !string.IsNullOrWhiteSpace(Query.Product)
        || !string.IsNullOrWhiteSpace(Query.Status)
        || !string.IsNullOrWhiteSpace(Query.Priority)
        || !string.IsNullOrWhiteSpace(Query.HasActions)
        || (!string.IsNullOrWhiteSpace(Query.Limit) && Query.Limit is not "100");
}
