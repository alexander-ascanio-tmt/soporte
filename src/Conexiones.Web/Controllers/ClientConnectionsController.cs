using Conexiones.Web.Data;
using Conexiones.Web.Models.Entities;
using Conexiones.Web.Models.TeamSupport;
using Conexiones.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Conexiones.Web.Controllers;

[Authorize]
public class ClientConnectionsController(
    ApplicationDbContext dbContext,
    TeamSupportDbContext teamSupportDbContext,
    UserManager<IdentityUser> userManager) : Controller
{
    private const int MaxClientSearchResults = 100;

    public async Task<IActionResult> Index()
    {
        var connections = await dbContext.ClientConnections
            .Include(c => c.ConnectionType)
            .OrderByDescending(c => c.RegisteredAt)
            .ToListAsync();

        return View(connections);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var connection = await dbContext.ClientConnections
            .AsNoTracking()
            .Include(c => c.ConnectionType)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (connection is null)
        {
            return NotFound();
        }

        return View(connection);
    }

    public async Task<IActionResult> Register(string? searchName)
    {
        var model = await BuildRegisterViewModelAsync(searchName);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterClientConnectionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateRegisterListsAsync(model, model.SearchName);
            return View(model);
        }

        var client = await teamSupportDbContext.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == model.TeamSupportClientId);

        if (client is null)
        {
            ModelState.AddModelError(nameof(model.TeamSupportClientId), "El cliente seleccionado no existe en TeamSupport.");
            await PopulateRegisterListsAsync(model, model.SearchName);
            return View(model);
        }

        var typeExists = await dbContext.ConnectionTypes
            .AnyAsync(t => t.Id == model.ConnectionTypeId && t.IsActive);

        if (!typeExists)
        {
            ModelState.AddModelError(nameof(model.ConnectionTypeId), "El tipo de conexión seleccionado no es válido.");
            await PopulateRegisterListsAsync(model, model.SearchName);
            return View(model);
        }

        var duplicate = await dbContext.ClientConnections
            .AnyAsync(c => c.TeamSupportClientId == client.CustomerId && c.ConnectionTypeId == model.ConnectionTypeId);

        if (duplicate)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un registro para este cliente y tipo de conexión.");
            await PopulateRegisterListsAsync(model, model.SearchName);
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);

        var connection = new ClientConnection
        {
            TeamSupportClientId = client.CustomerId,
            TeamSupportClientName = client.DisplayName,
            TeamSupportClientCode = client.ErpId,
            TeamSupportClientRazonSocial = client.RazonSocial,
            ConnectionTypeId = model.ConnectionTypeId,
            Notes = model.Notes,
            RegisteredByUserId = user?.Id,
            RegisteredByUserName = User.Identity?.Name
        };

        dbContext.ClientConnections.Add(connection);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"Conexión registrada para el cliente {client.DisplayName}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var connection = await dbContext.ClientConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (connection is null)
        {
            return NotFound();
        }

        var model = new EditClientConnectionViewModel
        {
            Id = connection.Id,
            TeamSupportClientName = connection.TeamSupportClientName,
            TeamSupportClientCode = connection.TeamSupportClientCode,
            TeamSupportClientRazonSocial = connection.TeamSupportClientRazonSocial,
            ConnectionTypeId = connection.ConnectionTypeId,
            Notes = connection.Notes
        };

        await PopulateEditListsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditClientConnectionViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateEditListsAsync(model);
            return View(model);
        }

        var connection = await dbContext.ClientConnections.FindAsync(id);
        if (connection is null)
        {
            return NotFound();
        }

        var typeExists = await dbContext.ConnectionTypes
            .AnyAsync(t => t.Id == model.ConnectionTypeId && t.IsActive);

        if (!typeExists)
        {
            ModelState.AddModelError(nameof(model.ConnectionTypeId), "El tipo de conexión seleccionado no es válido.");
            await PopulateEditListsAsync(model);
            return View(model);
        }

        var duplicate = await dbContext.ClientConnections
            .AnyAsync(c =>
                c.TeamSupportClientId == connection.TeamSupportClientId &&
                c.ConnectionTypeId == model.ConnectionTypeId &&
                c.Id != id);

        if (duplicate)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un registro para este cliente y tipo de conexión.");
            await PopulateEditListsAsync(model);
            return View(model);
        }

        connection.ConnectionTypeId = model.ConnectionTypeId;
        connection.Notes = model.Notes;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"Conexión actualizada para {connection.TeamSupportClientName}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateEditListsAsync(EditClientConnectionViewModel model)
    {
        model.ConnectionTypes = await dbContext.ConnectionTypes
            .AsNoTracking()
            .Where(t => t.IsActive || t.Id == model.ConnectionTypeId)
            .OrderBy(t => t.Name)
            .Select(t => new ConnectionTypeOption { Id = t.Id, Name = t.Name })
            .ToListAsync();
    }

    private async Task<RegisterClientConnectionViewModel> BuildRegisterViewModelAsync(string? searchName)
    {
        var model = new RegisterClientConnectionViewModel
        {
            SearchName = searchName
        };
        await PopulateRegisterListsAsync(model, searchName);
        return model;
    }

    private async Task PopulateRegisterListsAsync(
        RegisterClientConnectionViewModel model,
        string? searchName)
    {
        model.SearchName = searchName;

        if (model.HasSearch)
        {
            model.AvailableClients = await SearchClientsAsync(searchName);
        }
        else
        {
            model.AvailableClients = [];
        }

        model.ConnectionTypes = await dbContext.ConnectionTypes
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new ConnectionTypeOption { Id = t.Id, Name = t.Name })
            .ToListAsync();
    }

    private async Task<IReadOnlyList<TeamSupportClient>> SearchClientsAsync(string? searchName)
    {
        var name = searchName?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return [];
        }

        var namePattern = $"%{EscapeLikePattern(name)}%";

        return await teamSupportDbContext.Clients
            .AsNoTracking()
            .Where(c =>
                (c.CompanyName != null && EF.Functions.ILike(c.CompanyName, namePattern)) ||
                (c.RazonSocial != null && EF.Functions.ILike(c.RazonSocial, namePattern)))
            .OrderBy(c => c.CompanyName)
            .ThenBy(c => c.RazonSocial)
            .Take(MaxClientSearchResults)
            .ToListAsync();
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
