using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ProviderDTO
{
    [DisplayField("Id", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("SSN")]
    public string? Ssn { get; set; }

    [DisplayField("Last Name")]
    public string? LastName { get; set; }

    [DisplayField("First Name")]
    public string? FirstName { get; set; }

    [DisplayField("Phone")]
    public string? Phone { get; set; }

    [DisplayField("Email")]
    public string? Email { get; set; }

    [DisplayField("Tax ID", browsable: false)]
    public string? TaxId { get; set; }

    [DisplayField("Birthdate", browsable: false)]
    public DateTime? Birthdate { get; set; }

    [DisplayField("NPI")]
    public string? NpiNumber { get; set; }

    [DisplayField("Liability Insurance Date", browsable: false)]
    public DateTime? LiabilityInsuranceDate { get; set; }

    [DisplayField("License")]
    public string? License1 { get; set; }

    [DisplayField("License 1 Expiration", browsable: false)]
    public DateTime? License1Expiration { get; set; }

    [DisplayField("License 2", browsable: false)]
    public string? License2 { get; set; }

    [DisplayField("License 2 Expiration", browsable: false)]
    public DateTime? License2Expiration { get; set; }

    [DisplayField("Medical Date", browsable: false)]
    public DateTime? MedicalDate { get; set; }

    [DisplayField("Has Pets", browsable: false)]
    public bool HasPets { get; set; }

    [DisplayField("W9 Date", browsable: false)]
    public DateTime? W9Date { get; set; }

    [DisplayField("Direct Deposit Date", browsable: false)]
    public DateTime? DirectDepositDate { get; set; }

    [DisplayField("Contract Date", browsable: false)]
    public DateTime? ContractDate { get; set; }

    [DisplayField("Photo ID Date", browsable: false)]
    public DateTime? PhotoIdDate { get; set; }

    [DisplayField("Resume Date", browsable: false)]
    public DateTime? ResumeDate { get; set; }

    [DisplayField("HR Bundle Date", browsable: false)]
    public DateTime? HrBundleDate { get; set; }

    [DisplayField("Proof of Corp Date", browsable: false)]
    public DateTime? ProofOfCorpDate { get; set; }

    [DisplayField("Policies Date", browsable: false)]
    public DateTime? PoliciesDate { get; set; }

    [DisplayField("Medicaid Date", browsable: false)]
    public DateTime? MedicaidDate { get; set; }

    [DisplayField("Sexual Harassment Training Date", browsable: false)]
    public DateTime? SexualHarassmentTrainingDate { get; set; }

    [DisplayField("Corp Name")]
    public string? CorporationName { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Is Active")]
    public bool? IsActive { get; set; }

    [DisplayField("Address", browsable: false)]
    public string? Address { get; set; }

    [DisplayField("City", browsable: false)]
    public string? City { get; set; }

    [DisplayField("State", browsable: false)]
    public string? State { get; set; }

    [DisplayField("Zipcode", browsable: false)]
    public string? Zipcode { get; set; }

    [DisplayField("BL Ext Date", browsable: false)]
    public DateTime? BlExtDate { get; set; }

    [DisplayField("Languages", browsable: false)]
    public string? Languages { get; set; }

    [DisplayField("Direct Deposit Info", browsable: false)]
    public string? DirectDepositInfo { get; set; }

    [DisplayField("Is Duplicate Name", browsable: false)]
    public bool IsDuplicateName { get; set; }

    [DisplayField("Students")]
    public int AssignedCount { get; set; }
}
