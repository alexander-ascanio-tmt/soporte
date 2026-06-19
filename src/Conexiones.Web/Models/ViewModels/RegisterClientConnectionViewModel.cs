using System.ComponentModel.DataAnnotations;
using Conexiones.Web.Models.TeamSupport;

namespace Conexiones.Web.Models.ViewModels;

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

    [Display(Name = "Nombre o razón social")]
    public string? SearchName { get; set; }

    public bool HasSearch => !string.IsNullOrWhiteSpace(SearchName);

    public IReadOnlyList<TeamSupportClient> AvailableClients { get; set; } = [];

    public IReadOnlyList<ConnectionTypeOption> ConnectionTypes { get; set; } = [];
}

public class ConnectionTypeOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
