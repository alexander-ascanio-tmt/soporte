using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Soporte.Web.Data;
using Soporte.Web.Models.TeamSupport;
using Soporte.Web.Models.ViewModels;

namespace Soporte.Web.Services;

public class TicketSummaryService(TeamSupportDbContext teamSupportDbContext) : ITicketSummaryService
{
    private const int MaxClientSearchResults = 100;
    private const int MaxTickets = 50;
    private const int MaxDescriptionLength = 4000;
    private const int MaxActionLength = 2000;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<TicketSummaryViewModel> SearchClientsAsync(
        TicketSummaryViewModel model,
        CancellationToken cancellationToken = default)
    {
        model.SearchName = model.SearchName?.Trim();
        model.AvailableClients = await SearchClientsAsync(model.SearchName, cancellationToken);
        return model;
    }

    public async Task<TicketSummaryViewModel> BuildMoneyPennyPayloadAsync(
        TicketSummaryViewModel model,
        CancellationToken cancellationToken = default)
    {
        model.SearchName = model.SearchName?.Trim();
        model.Topic = model.Topic?.Trim();
        model.AvailableClients = await SearchClientsAsync(model.SearchName, cancellationToken);

        if (string.IsNullOrWhiteSpace(model.TeamSupportClientId))
        {
            throw new InvalidOperationException("Seleccione un cliente de TeamSupport.");
        }

        if (string.IsNullOrWhiteSpace(model.Topic))
        {
            throw new InvalidOperationException("Indique sobre qué desea obtener el resumen.");
        }

        var client = model.AvailableClients
            .FirstOrDefault(c => string.Equals(c.CustomerId, model.TeamSupportClientId, StringComparison.Ordinal))
            ?? await teamSupportDbContext.Clients
                .FirstOrDefaultAsync(c => c.CustomerId == model.TeamSupportClientId, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("El cliente seleccionado no existe en TeamSupport.");
        }

        model.SelectedClientName = client.DisplayName;
        model.AvailableClients = [];

        var (from, to) = ResolvePeriod(model);
        model.DateFrom = from.Date;
        model.DateTo = to.Date;

        var tickets = await LoadTicketsAsync(client, model.Topic, from, to, cancellationToken);
        var ticketIds = tickets.Select(t => t.Id).ToList();
        var actions = ticketIds.Count == 0
            ? []
            : await teamSupportDbContext.TicketActions
                .Where(a => ticketIds.Contains(a.TicketId))
                .OrderBy(a => a.CreatedAt)
                .ToListAsync(cancellationToken);

        var actionsByTicket = actions
            .GroupBy(a => a.TicketId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var payload = new
        {
            source = "MoneyPenny",
            generatedAt = DateTime.UtcNow,
            customer = new
            {
                id = client.CustomerId,
                companyName = client.CompanyName,
                razonSocial = client.RazonSocial,
                erpId = client.ErpId
            },
            filters = new
            {
                topic = model.Topic,
                period = model.PeriodPreset,
                dateFrom = from.ToString("yyyy-MM-dd"),
                dateTo = to.ToString("yyyy-MM-dd")
            },
            ticketCount = tickets.Count,
            tickets = tickets.Select(ticket => new
            {
                number = ticket.Number,
                subject = ticket.Title,
                description = Truncate(StripHtml(ticket.Description), MaxDescriptionLength),
                status = ticket.Status,
                priority = ticket.Priority,
                product = ticket.Product,
                customer = ticket.Customer,
                createdAt = ticket.CreatedAt,
                updatedAt = ticket.UpdatedAt,
                actions = actionsByTicket.GetValueOrDefault(ticket.Id, [])
                    .Select(action => new
                    {
                        actionType = action.ActionType,
                        createdBy = action.CreatedByName,
                        createdAt = action.CreatedAt,
                        content = Truncate(StripHtml(action.Content), MaxActionLength)
                    })
                    .ToList()
            }).ToList()
        };

        model.MoneyPennyPayload = JsonSerializer.Serialize(payload, JsonOptions);
        return model;
    }

    private async Task<IReadOnlyList<TeamSupportClient>> SearchClientsAsync(
        string? searchName,
        CancellationToken cancellationToken)
    {
        var name = searchName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return [];
        }

        var pattern = $"%{EscapeLikePattern(name)}%";
        return await teamSupportDbContext.Clients
            .Where(c =>
                (c.CompanyName != null && EF.Functions.ILike(c.CompanyName, pattern)) ||
                (c.RazonSocial != null && EF.Functions.ILike(c.RazonSocial, pattern)) ||
                (c.ErpId != null && EF.Functions.ILike(c.ErpId, pattern)))
            .OrderBy(c => c.CompanyName)
            .ThenBy(c => c.RazonSocial)
            .Take(MaxClientSearchResults)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<TeamSupportTicket>> LoadTicketsAsync(
        TeamSupportClient client,
        string topic,
        DateTime from,
        DateTime toExclusive,
        CancellationToken cancellationToken)
    {
        var topicPattern = $"%{EscapeLikePattern(topic)}%";
        var customerPatterns = new[] { client.CompanyName, client.RazonSocial, client.DisplayName }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => $"%{EscapeLikePattern(value!.Trim())}%")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var query = teamSupportDbContext.Tickets
            .Where(t => t.CreatedAt >= from && t.CreatedAt < toExclusive);

        if (customerPatterns.Count > 0)
        {
            query = query.Where(t =>
                t.Customer != null &&
                (EF.Functions.ILike(t.Customer, customerPatterns[0])
                 || (customerPatterns.Count > 1 && EF.Functions.ILike(t.Customer, customerPatterns[1]))
                 || (customerPatterns.Count > 2 && EF.Functions.ILike(t.Customer, customerPatterns[2]))));
        }

        query = query.Where(t =>
            (t.Title != null && EF.Functions.ILike(t.Title, topicPattern)) ||
            (t.Description != null && EF.Functions.ILike(t.Description, topicPattern)) ||
            (t.Product != null && EF.Functions.ILike(t.Product, topicPattern)));

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(MaxTickets)
            .ToListAsync(cancellationToken);
    }

    private static (DateTime From, DateTime ToExclusive) ResolvePeriod(TicketSummaryViewModel model)
    {
        var today = DateTime.Today;
        var toExclusive = (model.DateTo?.Date ?? today).AddDays(1);

        if (TryPresetStart(model.PeriodPreset, today, out var presetFrom))
        {
            return (presetFrom, today.AddDays(1));
        }

        if (model.DateFrom.HasValue)
        {
            return (model.DateFrom.Value.Date, toExclusive);
        }

        throw new InvalidOperationException("Indique un periodo o las fechas desde y hasta.");
    }

    private static bool TryPresetStart(string? preset, DateTime today, out DateTime from)
    {
        from = preset switch
        {
            "3d" => today.AddDays(-3),
            "5d" => today.AddDays(-5),
            "1w" => today.AddDays(-7),
            "1m" => today.AddMonths(-1),
            "3m" => today.AddMonths(-3),
            "6m" => today.AddMonths(-6),
            "1y" => today.AddYears(-1),
            _ => default
        };
        return from != default;
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private static string Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max] + "…";
    }

    private static string StripHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var withoutTags = Regex.Replace(value, "<.*?>", " ");
        return Regex.Replace(withoutTags, @"\s+", " ").Trim();
    }
}
