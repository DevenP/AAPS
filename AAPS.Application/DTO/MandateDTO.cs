namespace AAPS.Application.DTO;

public record MandateDTO
{
    public int Id { get; init; }
    public DateTime? ConferenceDate { get; init; }
    public string? StudentId { get; init; }
    public string? LastName { get; init; }
    public string? FirstName { get; init; }
    public string? HomeDistrict { get; init; }
    public string? Cse { get; init; }
    public string? CseDistrict { get; init; }
    public string? Grade { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? AdminDbn { get; init; }
    public string? D75 { get; init; }
    public string? ServiceType { get; init; }
    public string? Language { get; init; }
    public string? GroupSize { get; init; }
    public string? Duration { get; init; }
    public string? ServiceLocation { get; init; }
    public string? RemainingFrequency { get; init; }
    public string? Provider { get; init; }
    public DateTime? FirstAttendDate { get; init; }
    public string? MandateId { get; init; }
    public string? PrimaryPhone1 { get; init; }
    public string? PrimaryPhone2 { get; init; }
    public DateTime? MandateStart { get; init; }
    public DateTime? MandateEnd { get; init; }
    public string? FileName { get; init; }
    public int? RowNumber { get; set; }
    public DateTime? ServiceStartDate { get; init; }
}
