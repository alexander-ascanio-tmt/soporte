namespace Soporte.Web.Models.ViewModels;

public class UrgentTicketListItemViewModel
{
    public string TicketNumber { get; init; } = string.Empty;
    public string DisplayNumber { get; init; } = string.Empty;
    public string Subject { get; init; } = "—";
    public string Description { get; init; } = string.Empty;
    public string Customer { get; init; } = "—";
    public string Product { get; init; } = "—";
    public string Status { get; init; } = "—";
    public string Priority { get; init; } = "—";
    public DateTime? CreatedAt { get; init; }
    public DateTime UrgentAt { get; init; }
    public bool HasActions { get; init; }
    public bool FoundInTeamSupport { get; init; }
}
