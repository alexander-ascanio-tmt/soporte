using Conexiones.Web.Data;
using Conexiones.Web.Models.Entities;
using Conexiones.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Conexiones.Web.Controllers;

[Authorize]
public class ConnectionTypesController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var types = await dbContext.ConnectionTypes
            .OrderBy(t => t.Name)
            .ToListAsync();

        return View(types);
    }

    public IActionResult Create() => View(new ConnectionType());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConnectionType model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedAt = DateTime.UtcNow;
        dbContext.ConnectionTypes.Add(model);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Tipo de conexión creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var type = await dbContext.ConnectionTypes.FindAsync(id);
        if (type is null)
        {
            return NotFound();
        }

        return View(type);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConnectionType model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        dbContext.Update(model);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Tipo de conexión actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var type = await dbContext.ConnectionTypes
            .Include(t => t.ClientConnections)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type is null)
        {
            return NotFound();
        }

        return View(type);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var type = await dbContext.ConnectionTypes
            .Include(t => t.ClientConnections)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type is null)
        {
            return NotFound();
        }

        if (type.ClientConnections.Count > 0)
        {
            TempData["Error"] = "No se puede eliminar un tipo de conexión con registros asociados.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        dbContext.ConnectionTypes.Remove(type);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Tipo de conexión eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
