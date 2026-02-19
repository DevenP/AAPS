namespace AAPS.Application.DTO;

public record SesiDTO
{
    public int Id { get; init; }
    public string? StudentId { get; init; }
    public string? LastName { get; init; }
    public string? FirstName { get; init; }
    public string? Grade { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? HomeDistrict { get; init; }
    public string? Cse { get; init; }
    public string? CseDistrict { get; init; }
    public string? AdminDbn { get; init; }
    public string? GDistrict { get; init; }
    public string? Borough { get; init; }
    public string? MandateShort { get; init; }
    public DateTime? MandateStart { get; init; }
    public string? MandatedMaxGroup { get; init; }
    public DateTime? AssignmentFirstEncounter { get; init; }
    public string? AssignmentClaimed { get; init; }
    public DateTime? DateOfService { get; init; }
    public string? ServiceType { get; init; }
    public string? LanguageProvided { get; init; }
    public string? SessionType { get; init; }
    public string? SessionNotes { get; init; }
    public string? Grouping { get; init; }
    public string? ActualSize { get; init; }
    public string? StartTime { get; init; }
    public string? EndTime { get; init; }
    public string? Duration { get; init; }
    public string? ProviderLastName { get; init; }
    public string? ProviderFirstName { get; init; }
    public string? FileName { get; init; }
    public int? RowNumber { get; init; }
    public int? ProviderId { get; init; }
    public int? EntryId { get; init; }
    public decimal? BilledRate { get; init; }
    public decimal? BilledAmount { get; init; }
    public DateTime? BilledDate { get; init; }
    public DateTime? BilledPaidDate { get; init; }
    public decimal? ProviderRate { get; init; }
    public decimal? ProviderAmount { get; init; }
    public DateTime? ProviderPaidDate { get; init; }
    public bool IsOverlap { get; init; }
    public string? Voucher { get; init; }
    public bool IsOverMandate { get; init; }
    public bool IsOverDuration { get; init; }
    public bool IsUnderGroup { get; init; }
}
