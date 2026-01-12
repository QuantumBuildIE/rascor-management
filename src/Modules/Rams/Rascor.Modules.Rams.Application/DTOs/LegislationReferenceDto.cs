namespace Rascor.Modules.Rams.Application.DTOs;

public record LegislationReferenceDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ShortName { get; init; }
    public string? Description { get; init; }
    public string? Jurisdiction { get; init; }
    public string? Keywords { get; init; }
    public string? DocumentUrl { get; init; }
    public string? ApplicableCategories { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}
