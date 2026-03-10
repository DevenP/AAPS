using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ProviderDTO
{
    [ReadOnly(true)]
    [DisplayField("Id", browsable:true)]
    public int Id { get; set; }

    [DisplayField("Social")]
    public string? Ssn { get; set; }

    [DisplayField("Last Name")]
    public string? LastName { get; set; }

    [DisplayField("First Name")]
    public string? FirstName { get; set; }

    [DisplayField("Phone")]
    public string? Phone { get; set; }

    [DisplayField("Email")]
    public string? Email { get; set; }

    [DisplayField("Tax ID")]
    public string? TaxId { get; set; }

    [DisplayField("Birthdate")]
    public DateTime? Birthdate { get; set; }

    [DisplayField("NPI Number")]
    public string? NpiNumber { get; set; }

    [DisplayField("Liability Insurance Date")]
    public DateTime? LiabilityInsuranceDate { get; set; }

    [DisplayField("License 1")]
    public string? License1 { get; set; }

    [DisplayField("License 1 Expiration")]
    public DateTime? License1Expiration { get; set; }

    [DisplayField("License 2")]
    public string? License2 { get; set; }

    [DisplayField("License 2 Expiration")]
    public DateTime? License2Expiration { get; set; }

    [DisplayField("Medical Date")]
    public DateTime? MedicalDate { get; set; }

    [DisplayField("Has Pets")]
    public bool HasPets { get; set; }

    [DisplayField("W9 Date")]
    public DateTime? W9Date { get; set; }

    [DisplayField("Direct Deposit Date")]
    public DateTime? DirectDepositDate { get; set; }

    [DisplayField("Contract Date")]
    public DateTime? ContractDate { get; set; }

    [DisplayField("Photo ID Date")]
    public DateTime? PhotoIdDate { get; set; }

    [DisplayField("Resume Date")]
    public DateTime? ResumeDate { get; set; }

    [DisplayField("HR Bundle Date")]
    public DateTime? HrBundleDate { get; set; }

    [DisplayField("Proof of Corp Date")]
    public DateTime? ProofOfCorpDate { get; set; }

    [DisplayField("Policies Date")]
    public DateTime? PoliciesDate { get; set; }

    [DisplayField("Medicaid Date")]
    public DateTime? MedicaidDate { get; set; }

    [DisplayField("Sexual Harassment Training Date")]
    public DateTime? SexualHarassmentTrainingDate { get; set; }

    [DisplayField("Corporation Name")]
    public string? CorporationName { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Is Active")]
    public bool? IsActive { get; set; }

    [DisplayField("Address")]
    public string? Address { get; set; }

    [DisplayField("City")]
    public string? City { get; set; }

    [DisplayField("State")]
    public string? State { get; set; }

    [DisplayField("Zipcode")]
    public string? Zipcode { get; set; }

    [DisplayField("BL Ext Date")]
    public DateTime? BlExtDate { get; set; }

    [DisplayField("Languages")]
    public string? Languages { get; set; }

    [DisplayField("Direct Deposit Info")]
    public string? DirectDepositInfo { get; set; }

    [DisplayField("Is Duplicate Name")]
    public bool IsDuplicateName { get; set; }

    [DisplayField("Assigned Count")]
    public int AssignedCount { get; set; }
}
