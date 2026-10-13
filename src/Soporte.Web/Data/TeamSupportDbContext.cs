using Soporte.Web.Models.TeamSupport;
using Microsoft.EntityFrameworkCore;

namespace Soporte.Web.Data;

public class TeamSupportDbContext(DbContextOptions<TeamSupportDbContext> options) : DbContext(options)
{
    public DbSet<TeamSupportClient> Clients => Set<TeamSupportClient>();
    public DbSet<TeamSupportTicket> Tickets => Set<TeamSupportTicket>();
    public DbSet<TeamSupportTicketAction> TicketActions => Set<TeamSupportTicketAction>();

    public override int SaveChanges() =>
        throw new InvalidOperationException("teamsupport_db es de solo lectura.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("teamsupport_db es de solo lectura.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamSupportClient>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.CustomerId);

            entity.Property(e => e.CustomerId)
                .HasColumnName("CustomerId")
                .HasMaxLength(50);

            entity.Property(e => e.CompanyName)
                .HasColumnName("CompanyName");

            entity.Property(e => e.ErpId)
                .HasColumnName("ErpId")
                .HasMaxLength(100);

            entity.Property(e => e.RazonSocial)
                .HasColumnName("RazonSocial");

            entity.Property(e => e.TipoCliente)
                .HasColumnName("TipoCliente")
                .HasMaxLength(100);

            entity.Property(e => e.ValsoftCompany)
                .HasColumnName("ValsoftCompany")
                .HasMaxLength(100);
        });

        modelBuilder.Entity<TeamSupportTicket>(entity =>
        {
            entity.ToTable("tickets");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("Id");
            entity.Property(t => t.Number).HasColumnName("TicketNumber").HasMaxLength(50);
            entity.Property(t => t.Title).HasColumnName("Subject").HasMaxLength(500);
            entity.Property(t => t.Description).HasColumnName("Description");
            entity.Property(t => t.Status).HasColumnName("Status").HasMaxLength(50);
            entity.Property(t => t.Priority).HasColumnName("Priority").HasMaxLength(50);
            entity.Property(t => t.Customer).HasColumnName("CustomerName");
            entity.Property(t => t.Product).HasColumnName("ProductName");
            entity.Property(t => t.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(t => t.UpdatedAt).HasColumnName("UpdatedAt");
        });

        modelBuilder.Entity<TeamSupportTicketAction>(entity =>
        {
            entity.ToTable("ticket_actions");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("Id");
            entity.Property(a => a.TicketId).HasColumnName("TicketId");
            entity.Property(a => a.ActionType).HasColumnName("ActionType");
            entity.Property(a => a.Content).HasColumnName("Content");
            entity.Property(a => a.CreatedByName).HasColumnName("CreatedByName");
            entity.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
        });
    }
}
