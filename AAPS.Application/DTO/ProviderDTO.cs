namespace AAPS.Application.DTO;

public record ProviderDTO
{
    public int Id { get; init; }
    public string? Ssn { get; init; }
    public string? LastName { get; init; }
    public string? FirstName { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? TaxId { get; init; }
    public DateTime? Birthdate { get; init; }
    public string? NpiNumber { get; init; }
    public DateTime? LiabilityInsuranceDate { get; init; }
    public string? License1 { get; init; }
    public DateTime? License1Expiration { get; init; }
    public string? License2 { get; init; }
    public DateTime? License2Expiration { get; init; }
    public DateTime? MedicalDate { get; init; }
    public bool HasPets { get; init; }
    public DateTime? W9Date { get; init; }
    public DateTime? DirectDepositDate { get; init; }
    public DateTime? ContractDate { get; init; }
    public DateTime? PhotoIdDate { get; init; }
    public DateTime? ResumeDate { get; init; }
    public DateTime? HrBundleDate { get; init; }
    public DateTime? ProofOfCorpDate { get; init; }
    public DateTime? PoliciesDate { get; init; }
    public DateTime? MedicaidDate { get; init; }
    public DateTime? SexualHarassmentTrainingDate { get; init; }
    public string? CorporationName { get; init; }
    public string? ServiceType { get; init; }
    public string? Status { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Zipcode { get; init; }
    public DateTime? BlExtDate { get; init; }
    public string? Languages { get; init; }
    public string? DirectDepositInfo { get; init; }
}
