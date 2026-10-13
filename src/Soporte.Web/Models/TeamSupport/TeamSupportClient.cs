using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conexiones.Web.Models.TeamSupport;

/// <summary>
/// Entidad de solo lectura mapeada a public.customers en teamsupport_db (TeamSupport / Telematel).
/// </summary>
[Table("customers")]
public class TeamSupportClient
{
    [Key]
    [Column("CustomerId")]
    [Display(Name = "ID cliente")]
    public string CustomerId { get; set; } = string.Empty;

    [Column("CompanyName")]
    [Display(Name = "Empresa")]
    public string? CompanyName { get; set; }

    [Column("ErpId")]
    [Display(Name = "ERP ID")]
    public string? ErpId { get; set; }

    [Column("RazonSocial")]
    [Display(Name = "Razón social")]
    public string? RazonSocial { get; set; }

    [Column("TipoCliente")]
    [Display(Name = "Tipo cliente")]
    public string? TipoCliente { get; set; }

    [Column("ValsoftCompany")]
    [Display(Name = "Compañía Valsoft")]
    public string? ValsoftCompany { get; set; }

    [NotMapped]
    public string DisplayName => CompanyName ?? RazonSocial ?? CustomerId;
}
