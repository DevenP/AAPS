namespace AAPS.Application.DTO;

public record ImportLogDTO
{
    public int Id { get; init; }
    public string? ImportRecord { get; init; }
    public DateTime? ImportDate { get; init; }
    public string? FileName { get; init; }
}
