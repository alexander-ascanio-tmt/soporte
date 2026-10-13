using System.ComponentModel.DataAnnotations;
using Soporte.Web.Models.TeamSupport;

namespace Soporte.Web.Models.ViewModels;

public class RegisterClientConnectionViewModel
{
    [Required(ErrorMessage = "Seleccione un cliente de TeamSupport.")]
    [Display(Name = "Cliente")]
    public string? TeamSupportClientId { get; set; }

    [Required(ErrorMessage = "Seleccione un tipo de conexión.")]
    [Display(Name = "Tipo de conexión")]
    public int ConnectionTypeId { get; set; }

    [StringLength(1000)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    [Display(Name = "Búsqueda de Cliente")]
    public string? SearchName { get; set; }

    public string? SelectedClientName { get; set; }

    public bool HasSearch => !string.IsNullOrWhiteSpace(SearchName);

    public IReadOnlyList<TeamSupportClient> AvailableClients { get; set; } = [];

    public IReadOnlyList<ConnectionTypeOption> ConnectionTypes { get; set; } = [];
}

public class ConnectionTypeOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
