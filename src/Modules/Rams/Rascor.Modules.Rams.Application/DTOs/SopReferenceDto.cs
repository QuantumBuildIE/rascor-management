namespace Rascor.Modules.Rams.Application.DTOs;

public record SopReferenceDto
{
    public Guid Id { get; init; }
    public string SopId { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? TaskKeywords { get; init; }
    public string? PolicySnippet { get; init; }
    public string? ProcedureDetails { get; init; }
    public string? ApplicableLegislation { get; init; }
    public string? DocumentUrl { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
}
