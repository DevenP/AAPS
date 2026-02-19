namespace AAPS.Application.DTO;

public record ServiceTypeDTO
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public bool IsEvaluation { get; init; }
}
