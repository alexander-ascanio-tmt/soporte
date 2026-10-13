using System.ComponentModel.DataAnnotations;

namespace Soporte.Web.Models.Entities;

public class TicketUrgency
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string TicketNumber { get; set; } = string.Empty;

    public bool IsUrgent { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(256)]
    public string? UpdatedByUserName { get; set; }
}
