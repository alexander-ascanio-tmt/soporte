using Conexiones.Web.Models.TeamSupport;
using Microsoft.EntityFrameworkCore;

namespace Conexiones.Web.Data;

public class TeamSupportDbContext(DbContextOptions<TeamSupportDbContext> options) : DbContext(options)
{
    public DbSet<TeamSupportClient> Clients => Set<TeamSupportClient>();

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
    }
}
