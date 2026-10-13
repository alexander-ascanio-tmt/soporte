namespace Soporte.Web.Models.TeamSupport;

/// <summary>
/// Entidad de solo lectura mapeada a public.tickets en teamsupport_db.
/// </summary>
public class TeamSupportTicket
{
    public int Id { get; set; }
    public string? Number { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Customer { get; set; }
    public string? Product { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
