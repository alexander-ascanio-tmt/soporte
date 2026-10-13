using System.ComponentModel.DataAnnotations;

namespace Soporte.Web.Models.Entities;

public class ConnectionType
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Tipo")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Contacto")]
    public string? Contact { get; set; }

    [StringLength(30)]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }

    [StringLength(256)]
    [EmailAddress]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ClientConnection> ClientConnections { get; set; } = [];
}
