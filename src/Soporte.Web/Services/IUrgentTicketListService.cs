using Soporte.Web.Models.ViewModels;

namespace Soporte.Web.Services;

public interface IUrgentTicketListService
{
    Task<UrgentTicketListPageViewModel> GetUrgentTicketsAsync(
        UrgentTicketListQuery query,
        CancellationToken cancellationToken = default);
}
