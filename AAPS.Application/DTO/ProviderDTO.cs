using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record ProviderDTO
{
    [ReadOnly(true)]
    [Browsable(true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "Social")]
    public string? Ssn { get; set; }

    [Browsable(true)]
    [Display(Name = "LastName")]
    public string? LastName { get; set; }

    [Browsable(true)]
    [Display(Name = "FirstName")]
    public string? FirstName { get; set; }

    [Browsable(true)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Browsable(true)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Browsable(true)]
    [Display(Name = "TaxId")]
    public string? TaxId { get; set; }

    [Browsable(true)]
    [Display(Name = "Birthdate")]
    public DateTime? Birthdate { get; set; }

    [Browsable(true)]
    [Display(Name = "NpiNumber")]
    public string? NpiNumber { get; set; }

    [Browsable(true)]
    [Display(Name = "LiabilityInsuranceDate")]
    public DateTime? LiabilityInsuranceDate { get; set; }

    [Browsable(true)]
    [Display(Name = "License1")]
    public string? License1 { get; set; }

    [Browsable(true)]
    [Display(Name = "License1Expiration")]
    public DateTime? License1Expiration { get; set; }

    [Browsable(true)]
    [Display(Name = "License2")]
    public string? License2 { get; set; }

    [Browsable(true)]
    [Display(Name = "License2Expiration")]
    public DateTime? License2Expiration { get; set; }

    [Browsable(true)]
    [Display(Name = "MedicalDate")]
    public DateTime? MedicalDate { get; set; }

    [Browsable(true)]
    [Display(Name = "HasPets")]
    public bool HasPets { get; set; }

    [Browsable(true)]
    [Display(Name = "W9Date")]
    public DateTime? W9Date { get; set; }

    [Browsable(true)]
    [Display(Name = "DirectDepositDate")]
    public DateTime? DirectDepositDate { get; set; }

    [Browsable(true)]
    [Display(Name = "ContractDate")]
    public DateTime? ContractDate { get; set; }

    [Browsable(true)]
    [Display(Name = "PhotoIdDate")]
    public DateTime? PhotoIdDate { get; set; }

    [Browsable(true)]
    [Display(Name = "ResumeDate")]
    public DateTime? ResumeDate { get; set; }

    [Browsable(true)]
    [Display(Name = "HrBundleDate")]
    public DateTime? HrBundleDate { get; set; }

    [Browsable(true)]
    [Display(Name = "ProofOfCorpDate")]
    public DateTime? ProofOfCorpDate { get; set; }

    [Browsable(true)]
    [Display(Name = "PoliciesDate")]
    public DateTime? PoliciesDate { get; set; }

    [Browsable(true)]
    [Display(Name = "MedicaidDate")]
    public DateTime? MedicaidDate { get; set; }

    [Browsable(true)]
    [Display(Name = "SexualHarassmentTrainingDate")]
    public DateTime? SexualHarassmentTrainingDate { get; set; }

    [Browsable(true)]
    [Display(Name = "CorporationName")]
    public string? CorporationName { get; set; }

    [Browsable(true)]
    [Display(Name = "ServiceType")]
    public string? ServiceType { get; set; }

    [Browsable(true)]
    [Display(Name = "IsActive")]
    public bool? IsActive { get; set; }

    [Browsable(true)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Browsable(true)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [Browsable(true)]
    [Display(Name = "State")]
    public string? State { get; set; }

    [Browsable(true)]
    [Display(Name = "Zipcode")]
    public string? Zipcode { get; set; }

    [Browsable(true)]
    [Display(Name = "BlExtDate")]
    public DateTime? BlExtDate { get; set; }

    [Browsable(true)]
    [Display(Name = "Languages")]
    public string? Languages { get; set; }

    [Browsable(true)]
    [Display(Name = "DirectDepositInfo")]
    public string? DirectDepositInfo { get; set; }

    //

    [Browsable(true)]
    [Display(Name = "IsDuplicateName")]
    public bool IsDuplicateName { get; set; }

    [Browsable(true)]
    [Display(Name = "AssignedCount")]
    public int AssignedCount { get; set; }
}
