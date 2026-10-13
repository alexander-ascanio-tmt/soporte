namespace Soporte.Web.Models.TeamSupport;

/// <summary>
/// Entidad de solo lectura mapeada a public.ticket_actions en teamsupport_db.
/// </summary>
public class TeamSupportTicketAction
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string? ActionType { get; set; }
    public string? Content { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}
