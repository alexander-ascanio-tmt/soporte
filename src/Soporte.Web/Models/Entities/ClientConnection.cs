using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Soporte.Web.Models.Entities;

public class ClientConnection
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Cliente TeamSupport")]
    public string TeamSupportClientId { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    [Display(Name = "Nombre del cliente")]
    public string TeamSupportClientName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "ERP ID")]
    public string? TeamSupportClientCode { get; set; }

    [StringLength(300)]
    [Display(Name = "Razón social")]
    public string? TeamSupportClientRazonSocial { get; set; }

    [Display(Name = "Tipo de conexión")]
    public int ConnectionTypeId { get; set; }

    [ForeignKey(nameof(ConnectionTypeId))]
    public ConnectionType ConnectionType { get; set; } = null!;

    [StringLength(1000)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [StringLength(256)]
    public string? RegisteredByUserId { get; set; }

    [StringLength(256)]
    [Display(Name = "Registrado por")]
    public string? RegisteredByUserName { get; set; }
}
