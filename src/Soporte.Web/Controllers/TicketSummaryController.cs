using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soporte.Web.Models.ViewModels;
using Soporte.Web.Services;

namespace Soporte.Web.Controllers;

[Authorize]
public class TicketSummaryController(ITicketSummaryService ticketSummaryService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new TicketSummaryViewModel
        {
            PeriodPreset = "1m"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(TicketSummaryViewModel model, string? intent, CancellationToken cancellationToken)
    {
        try
        {
            if (string.Equals(intent, "fetch", StringComparison.OrdinalIgnoreCase))
            {
                model = await ticketSummaryService.BuildMoneyPennyPayloadAsync(model, cancellationToken);
            }
            else
            {
                model = await ticketSummaryService.SearchClientsAsync(model, cancellationToken);
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model = await ticketSummaryService.SearchClientsAsync(model, cancellationToken);
        }

        return View(model);
    }
}
