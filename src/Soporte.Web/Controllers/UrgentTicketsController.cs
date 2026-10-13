using Soporte.Web.Models.ViewModels;
using Soporte.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Soporte.Web.Controllers;

[Authorize]
public class UrgentTicketsController(IUrgentTicketListService urgentTicketListService) : Controller
{
    public async Task<IActionResult> Index(
        string? search,
        string? customer,
        string? product,
        string? estado,
        string? priority,
        string? hasActions,
        string? limit,
        CancellationToken cancellationToken)
    {
        var query = new UrgentTicketListQuery
        {
            Search = search,
            Customer = customer,
            Product = product,
            Status = estado,
            Priority = priority,
            HasActions = hasActions,
            Limit = limit
        };

        var model = await urgentTicketListService.GetUrgentTicketsAsync(query, cancellationToken);
        return View(model);
    }
}
