namespace AAPS.Application.DTO;

public record EvalDTO
{
    public int Id { get; init; }
    public string? StudentFirstName { get; init; }
    public string? StudentLastName { get; init; }
    public string? StudentId { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? ParentFirstName { get; init; }
    public string? ParentLastName { get; init; }
    public DateTime? EvalReceivedDate { get; init; }
    public int? ProviderId { get; init; }
    public DateTime? AssignedDate { get; init; }
    public DateTime? ReportReceivedDate { get; init; }
    public DateTime? EvalDate { get; init; }
    public DateTime? ReportSubmittedDate { get; init; }
    public string? District { get; init; }
    public string? Language { get; init; }
    public string? ServiceType { get; init; }
    public string? Contact { get; init; }
    public decimal? ProviderAmount { get; init; }
    public decimal? BilledAmount { get; init; }
    public DateTime? ProviderPaidDate { get; init; }
    public DateTime? BilledPaidDate { get; init; }
    public DateTime? BilledDate { get; init; }
    public string? Memo { get; init; }
    public DateTime? AppointmentDate { get; init; }
    public string? Status { get; init; }

    public string? ProviderName { get; init; }
}
