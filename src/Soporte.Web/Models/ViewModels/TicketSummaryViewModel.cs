using System.ComponentModel.DataAnnotations;
using Soporte.Web.Models.TeamSupport;

namespace Soporte.Web.Models.ViewModels;

public class TicketSummaryViewModel
{
    [Display(Name = "Búsqueda de Cliente")]
    public string? SearchName { get; set; }

    [Display(Name = "Cliente")]
    public string? TeamSupportClientId { get; set; }

    public string? SelectedClientName { get; set; }

    [Display(Name = "Periodo")]
    public string? PeriodPreset { get; set; }

    [Display(Name = "Fecha desde")]
    [DataType(DataType.Date)]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "Fecha hasta")]
    [DataType(DataType.Date)]
    public DateTime? DateTo { get; set; }

    [Display(Name = "Sobre qué desea el resumen")]
    public string? Topic { get; set; }

    public string? MoneyPennyPayload { get; set; }

    public bool HasSearch => !string.IsNullOrWhiteSpace(SearchName);

    public IReadOnlyList<TeamSupportClient> AvailableClients { get; set; } = [];
}
