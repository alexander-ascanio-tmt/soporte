namespace Soporte.Web.Models.ViewModels;

public class UrgentTicketListQuery
{
    public string? Search { get; init; }
    public string? Customer { get; init; }
    public string? Product { get; init; }
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public string? HasActions { get; init; }
    public string? Limit { get; init; }
}
