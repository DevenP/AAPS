namespace AAPS.Application.DTO;

public record LanguageDTO
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Code { get; init; }
}
