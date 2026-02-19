namespace AAPS.Application.DTO
{
    public record VendorPortalDTO
    {
        public int Id { get; init; }
        public string? Ssn { get; init; }
        public string? Boro { get; init; }
        public string? District { get; init; }
        public string? School { get; init; }
        public string? Fund { get; init; }
        public string? StudentId { get; init; }
        public string? Duration { get; init; }
        public string? Frequency { get; init; }
        public string? GroupSize { get; init; }
        public DateTime? StartDate { get; init; }
        public string? AssignmentId { get; init; }
        public string? FileName { get; init; }
        public int? EntryId { get; init; }
    }
}
