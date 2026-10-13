using System.ComponentModel.DataAnnotations;

namespace Soporte.Web.Models.ViewModels;

public class EditClientConnectionViewModel
{
    public int Id { get; set; }

    [Display(Name = "Cliente")]
    public string TeamSupportClientName { get; set; } = string.Empty;

    [Display(Name = "ERP ID")]
    public string? TeamSupportClientCode { get; set; }

    [Display(Name = "Razón social")]
    public string? TeamSupportClientRazonSocial { get; set; }

    [Required(ErrorMessage = "Seleccione un tipo de conexión.")]
    [Display(Name = "Tipo de conexión")]
    public int ConnectionTypeId { get; set; }

    [StringLength(1000)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    public IReadOnlyList<ConnectionTypeOption> ConnectionTypes { get; set; } = [];
}
