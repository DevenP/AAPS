namespace AAPS.Application.DTO;

public record SemesterDTO
{
    public int Id { get; init; }
    public string Code { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
