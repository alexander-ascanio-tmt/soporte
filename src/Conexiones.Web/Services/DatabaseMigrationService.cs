using Conexiones.Web.Data;
using Conexiones.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conexiones.Web.Services;

public interface IDatabaseMigrationService
{
    Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);
}

public class DatabaseMigrationService(
    ApplicationDbContext applicationDbContext,
    ILogger<DatabaseMigrationService> logger) : IDatabaseMigrationService
{
    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Aplicando migraciones en soporte_ts_db...");
        await applicationDbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migraciones aplicadas correctamente.");
    }
}

public interface IConnectionTypeSeedService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

public class ConnectionTypeSeedService(
    ApplicationDbContext dbContext,
    ILogger<ConnectionTypeSeedService> logger) : IConnectionTypeSeedService
{
    private static readonly string[] DefaultTypes =
    [
        "API REST",
        "Base de datos",
        "VPN",
        "SFTP"
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.ConnectionTypes.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var name in DefaultTypes)
        {
            dbContext.ConnectionTypes.Add(new ConnectionType
            {
                Name = name,
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Tipos de conexión iniciales creados.");
    }
}
