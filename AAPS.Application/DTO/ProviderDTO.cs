using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ProviderDTO
{
    [DisplayField("Id", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("SSN", GroupName = "Info")]
    public string? Ssn { get; set; }

    [DisplayField("Last Name", GroupName = "Info")]
    public string? LastName { get; set; }

    [DisplayField("First Name", GroupName = "Info")]
    public string? FirstName { get; set; }

    [DisplayField("Phone", GroupName = "Info")]
    public string? Phone { get; set; }

    [DisplayField("Email", GroupName = "Info")]
    public string? Email { get; set; }

    [DisplayField("Tax ID", browsable: false)]
    public string? TaxId { get; set; }

    [DisplayField("Birthdate", browsable: false)]
    public DateTime? Birthdate { get; set; }

    [DisplayField("NPI", GroupName = "Info")]
    public string? NpiNumber { get; set; }

    [DisplayField("Liability Insurance Date", GroupName = "Compliance")]
    public DateTime? LiabilityInsuranceDate { get; set; }

    [DisplayField("License", GroupName = "Info")]
    public string? License1 { get; set; }

    [DisplayField("License 1 Expiration", GroupName = "Compliance")]
    public DateTime? License1Expiration { get; set; }

    [DisplayField("License 2", browsable: false)]
    public string? License2 { get; set; }

    [DisplayField("License 2 Expiration", GroupName = "Compliance")]
    public DateTime? License2Expiration { get; set; }

    [DisplayField("Medical Date", GroupName = "Compliance")]
    public DateTime? MedicalDate { get; set; }

    [DisplayField("Has PETS", browsable: false)]
    public bool HasPets { get; set; }

    [DisplayField("W9 / W4 Date", GroupName = "Compliance")]
    public DateTime? W9Date { get; set; }

    [DisplayField("Direct Deposit Date", GroupName = "Compliance")]
    public DateTime? DirectDepositDate { get; set; }

    [DisplayField("Contract Date", GroupName = "Compliance")]
    public DateTime? ContractDate { get; set; }

    [DisplayField("Photo ID Date", GroupName = "Compliance")]
    public DateTime? PhotoIdDate { get; set; }

    [DisplayField("Resume Date", GroupName = "Compliance")]
    public DateTime? ResumeDate { get; set; }

    [DisplayField("HR Bundle Date", GroupName = "Compliance")]
    public DateTime? HrBundleDate { get; set; }

    [DisplayField("Proof of Corp Date", GroupName = "Compliance")]
    public DateTime? ProofOfCorpDate { get; set; }

    [DisplayField("Policies Date", GroupName = "Compliance")]
    public DateTime? PoliciesDate { get; set; }

    [DisplayField("Medicaid Date", GroupName = "Compliance")]
    public DateTime? MedicaidDate { get; set; }

    [DisplayField("Sexual Harassment Training Date", GroupName = "Compliance")]
    public DateTime? SexualHarassmentTrainingDate { get; set; }

    [DisplayField("I-9 Date", GroupName = "Compliance")]
    public DateTime? I9Date { get; set; }

    [DisplayField("Para / Health Aide Training Certificate Date", GroupName = "Compliance")]
    public DateTime? ParaHealthAideCertDate { get; set; }

    [DisplayField("High School Diploma Date", GroupName = "Compliance")]
    public DateTime? HighSchoolDiplomaDate { get; set; }

    [DisplayField("Corp Name", GroupName = "Info")]
    public string? CorporationName { get; set; }

    [FilterOptionsSource(FilterSource.ServiceType)]
    [DisplayField("Service Type", GroupName = "Service")]
    public string? ServiceType { get; set; }

    [DisplayField("Is Active", GroupName = "Info")]
    public bool? IsActive { get; set; }

    [DisplayField("Address", browsable: false)]
    public string? Address { get; set; }

    [DisplayField("City", browsable: false)]
    public string? City { get; set; }

    [DisplayField("State", browsable: false)]
    public string? State { get; set; }

    [DisplayField("Zipcode", browsable: false)]
    public string? Zipcode { get; set; }

    [DisplayField("BL Ext Date", GroupName = "Compliance")]
    public DateTime? BlExtDate { get; set; }

    [DisplayField("Languages", browsable: false)]
    public string? Languages { get; set; }

    [DisplayField("Direct Deposit Info", browsable: false)]
    public string? DirectDepositInfo { get; set; }

    [DisplayField("Is Duplicate Name", browsable: false)]
    public bool IsDuplicateName { get; set; }

    [DisplayField("Students", GroupName = "Service")]
    public int AssignedCount { get; set; }
}
