using Conexiones.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Conexiones.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<ConnectionType> ConnectionTypes => Set<ConnectionType>();
    public DbSet<ClientConnection> ClientConnections => Set<ClientConnection>();
    public DbSet<TicketUrgency> TicketUrgencies => Set<TicketUrgency>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ConnectionType>(entity =>
        {
            entity.ToTable("connection_types");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Contact).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        builder.Entity<ClientConnection>(entity =>
        {
            entity.ToTable("client_connections");
            entity.HasIndex(e => new { e.TeamSupportClientId, e.ConnectionTypeId }).IsUnique();

            entity.HasOne(e => e.ConnectionType)
                .WithMany(t => t.ClientConnections)
                .HasForeignKey(e => e.ConnectionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketUrgency>(entity =>
        {
            entity.ToTable("ticket_urgencies");
            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.Property(e => e.TicketNumber).HasMaxLength(50);
            entity.Property(e => e.UpdatedByUserName).HasMaxLength(256);
        });
    }
}
