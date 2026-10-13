using Soporte.Web.Models.ViewModels;

namespace Soporte.Web.Services;

public interface ITicketSummaryService
{
    Task<TicketSummaryViewModel> SearchClientsAsync(
        TicketSummaryViewModel model,
        CancellationToken cancellationToken = default);

    Task<TicketSummaryViewModel> BuildMoneyPennyPayloadAsync(
        TicketSummaryViewModel model,
        CancellationToken cancellationToken = default);
}
