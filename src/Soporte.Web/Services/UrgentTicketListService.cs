using Soporte.Web.Data;
using Soporte.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Soporte.Web.Services;

public class UrgentTicketListService(
    ApplicationDbContext applicationDbContext,
    TeamSupportDbContext teamSupportDbContext,
    ILogger<UrgentTicketListService> logger) : IUrgentTicketListService
{
    public async Task<UrgentTicketListPageViewModel> GetUrgentTicketsAsync(
        UrgentTicketListQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = NormalizeQuery(query);
        var allTickets = await LoadUrgentTicketsAsync(cancellationToken);

        var filtered = allTickets.Where(ticket => Matches(ticket, normalizedQuery)).ToList();
        var visible = ApplyLimit(filtered, normalizedQuery.Limit);

        return new UrgentTicketListPageViewModel
        {
            Tickets = visible,
            Query = normalizedQuery,
            Customers = DistinctValues(allTickets, t => t.Customer),
            Products = DistinctValues(allTickets, t => t.Product),
            Statuses = DistinctValues(allTickets, t => t.Status),
            Priorities = DistinctValues(allTickets, t => t.Priority),
            TotalCount = filtered.Count,
            VisibleCount = visible.Count
        };
    }

    private async Task<List<UrgentTicketListItemViewModel>> LoadUrgentTicketsAsync(
        CancellationToken cancellationToken)
    {
        var urgencies = await applicationDbContext.TicketUrgencies
            .AsNoTracking()
            .Where(u => u.IsUrgent)
            .OrderByDescending(u => u.UpdatedAt)
            .ToListAsync(cancellationToken);

        if (urgencies.Count == 0)
        {
            return [];
        }

        var normalizedKeys = urgencies
            .Select(u => NormalizeNumber(u.TicketNumber))
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var lookupKeys = normalizedKeys
            .SelectMany(n => new[] { n, "#" + n })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var tickets = await teamSupportDbContext.Tickets
            .Where(t => t.Number != null && lookupKeys.Contains(t.Number))
            .ToListAsync(cancellationToken);

        var ticketIds = tickets.Select(t => t.Id).ToList();
        var actionCounts = ticketIds.Count == 0
            ? new Dictionary<int, int>()
            : await teamSupportDbContext.TicketActions
                .Where(a => ticketIds.Contains(a.TicketId))
                .GroupBy(a => a.TicketId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var ticketsByNumber = tickets
            .Where(t => !string.IsNullOrWhiteSpace(t.Number))
            .GroupBy(t => NormalizeNumber(t.Number!), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        logger.LogInformation(
            "Tickets urgentes: {UrgentCount} en soporte_db, {MatchedCount} encontrados en teamsupport_db.",
            urgencies.Count,
            ticketsByNumber.Count);

        return urgencies.Select(urgency =>
        {
            var key = NormalizeNumber(urgency.TicketNumber);
            ticketsByNumber.TryGetValue(key, out var ticket);
            var number = ticket?.Number ?? urgency.TicketNumber;
            var actionCount = ticket is null ? 0 : actionCounts.GetValueOrDefault(ticket.Id);

            return new UrgentTicketListItemViewModel
            {
                TicketNumber = number,
                DisplayNumber = FormatDisplayNumber(number),
                Subject = string.IsNullOrWhiteSpace(ticket?.Title) ? "—" : ticket.Title,
                Description = ticket?.Description ?? string.Empty,
                Customer = string.IsNullOrWhiteSpace(ticket?.Customer) ? "—" : ticket.Customer,
                Product = string.IsNullOrWhiteSpace(ticket?.Product) ? "—" : ticket.Product,
                Status = string.IsNullOrWhiteSpace(ticket?.Status) ? "—" : ticket.Status,
                Priority = string.IsNullOrWhiteSpace(ticket?.Priority) ? "—" : ticket.Priority,
                CreatedAt = ticket?.CreatedAt,
                UrgentAt = urgency.UpdatedAt,
                HasActions = actionCount > 1,
                FoundInTeamSupport = ticket is not null
            };
        }).ToList();
    }

    private static UrgentTicketListQuery NormalizeQuery(UrgentTicketListQuery query) =>
        new()
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            Customer = NullIfEmpty(query.Customer),
            Product = NullIfEmpty(query.Product),
            Status = NullIfEmpty(query.Status),
            Priority = NullIfEmpty(query.Priority),
            HasActions = NullIfEmpty(query.HasActions),
            Limit = query.Limit switch
            {
                "200" => "200",
                "all" => "all",
                _ => "100"
            }
        };

    private static bool Matches(UrgentTicketListItemViewModel ticket, UrgentTicketListQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search;
            var matchesSearch =
                Contains(ticket.TicketNumber, term)
                || Contains(ticket.DisplayNumber, term)
                || Contains(ticket.Subject, term)
                || Contains(ticket.Description, term);

            if (!matchesSearch)
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Customer) &&
            !string.Equals(ticket.Customer, query.Customer, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(query.Product) &&
            !string.Equals(ticket.Product, query.Product, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            !string.Equals(ticket.Status, query.Status, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(query.Priority) &&
            !string.Equals(ticket.Priority, query.Priority, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (query.HasActions is "true" && !ticket.HasActions)
        {
            return false;
        }

        if (query.HasActions is "false" && ticket.HasActions)
        {
            return false;
        }

        return true;
    }

    private static IReadOnlyList<UrgentTicketListItemViewModel> ApplyLimit(
        IReadOnlyList<UrgentTicketListItemViewModel> tickets,
        string? limit) =>
        limit switch
        {
            "200" => tickets.Take(200).ToList(),
            "all" => tickets,
            _ => tickets.Take(100).ToList()
        };

    private static IReadOnlyList<string> DistinctValues(
        IEnumerable<UrgentTicketListItemViewModel> tickets,
        Func<UrgentTicketListItemViewModel, string> selector) =>
        tickets
            .Select(selector)
            .Where(value => !string.IsNullOrWhiteSpace(value) && value != "—")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static bool Contains(string? value, string term) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeNumber(string ticketNumber) =>
        ticketNumber.Trim().TrimStart('#');

    private static string FormatDisplayNumber(string ticketNumber)
    {
        var normalized = NormalizeNumber(ticketNumber);
        return string.IsNullOrEmpty(normalized) ? "—" : $"#{normalized}";
    }
}
