namespace Soporte.Web.Models.ViewModels;

public class ClientConnectionListQuery
{
    public string? Search { get; init; }
    public int? ConnectionTypeId { get; init; }
    public string? RegisteredBy { get; init; }
    public string? Limit { get; init; }
}
