using Soporte.Web.Data;
using Soporte.Web.Models.Api;
using Soporte.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Soporte.Web.Services;

public class TicketUrgencyService(ApplicationDbContext dbContext) : ITicketUrgencyService
{
    public async Task<TicketUrgencyResponse> GetAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeTicketNumber(ticketNumber);
        var record = await dbContext.TicketUrgencies
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TicketNumber == normalized, cancellationToken);

        return ToResponse(normalized, record);
    }

    public async Task<TicketUrgencyResponse> SetAsync(
        string ticketNumber,
        bool isUrgent,
        string? updatedByUserName = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeTicketNumber(ticketNumber);
        var record = await dbContext.TicketUrgencies
            .FirstOrDefaultAsync(t => t.TicketNumber == normalized, cancellationToken);

        if (record is null)
        {
            record = new TicketUrgency
            {
                TicketNumber = normalized,
                IsUrgent = isUrgent,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserName = updatedByUserName
            };
            dbContext.TicketUrgencies.Add(record);
        }
        else
        {
            record.IsUrgent = isUrgent;
            record.UpdatedAt = DateTime.UtcNow;
            record.UpdatedByUserName = updatedByUserName;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(normalized, record);
    }

    public async Task<TicketUrgencyBatchResponse> GetBatchAsync(
        IEnumerable<string> ticketNumbers,
        CancellationToken cancellationToken = default)
    {
        var normalizedNumbers = ticketNumbers
            .Select(number => number.Trim())
            .Where(number => !string.IsNullOrWhiteSpace(number))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var response = new TicketUrgencyBatchResponse();

        if (normalizedNumbers.Count == 0)
        {
            return response;
        }

        var records = await dbContext.TicketUrgencies
            .AsNoTracking()
            .Where(record => normalizedNumbers.Contains(record.TicketNumber))
            .ToListAsync(cancellationToken);

        var recordsByNumber = records.ToDictionary(record => record.TicketNumber, StringComparer.Ordinal);

        foreach (var ticketNumber in normalizedNumbers)
        {
            recordsByNumber.TryGetValue(ticketNumber, out var record);
            response.Matches[ticketNumber] = ToResponse(ticketNumber, record);
        }

        return response;
    }

    private static string NormalizeTicketNumber(string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
        {
            throw new ArgumentException("El número de ticket es obligatorio.", nameof(ticketNumber));
        }

        return ticketNumber.Trim();
    }

    private static TicketUrgencyResponse ToResponse(string ticketNumber, TicketUrgency? record) =>
        new()
        {
            Ok = true,
            TicketNumber = ticketNumber,
            IsUrgent = record?.IsUrgent ?? false,
            UpdatedAt = record?.UpdatedAt
        };
}
