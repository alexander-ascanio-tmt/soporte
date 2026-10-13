using Soporte.Web.Models.Api;

namespace Soporte.Web.Services;

public interface ITicketUrgencyService
{
    Task<TicketUrgencyResponse> GetAsync(string ticketNumber, CancellationToken cancellationToken = default);

    Task<TicketUrgencyResponse> SetAsync(
        string ticketNumber,
        bool isUrgent,
        string? updatedByUserName = null,
        CancellationToken cancellationToken = default);

    Task<TicketUrgencyBatchResponse> GetBatchAsync(
        IEnumerable<string> ticketNumbers,
        CancellationToken cancellationToken = default);
}
