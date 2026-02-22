using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record SesiDTO
{
    [Browsable(true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "StudentId")]
    public string? StudentId { get; set; }

    [Browsable(true)]
    [Display(Name = "StudentLastName")]
    public string? StudentLastName { get; set; }

    [Browsable(true)]
    [Display(Name = "StudentFirstName")]
    public string? StudentFirstName { get; set; }

    [Browsable(true)]
    [Display(Name = "Grade")]
    public string? Grade { get; set; }

    [Browsable(true)]
    [Display(Name = "DateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [Browsable(true)]
    [Display(Name = "HomeDistrict")]
    public string? HomeDistrict { get; set; }

    [Browsable(true)]
    [Display(Name = "Cse")]
    public string? Cse { get; set; }

    [Browsable(true)]
    [Display(Name = "CseDistrict")]
    public string? CseDistrict { get; set; }

    [Browsable(true)]
    [Display(Name = "AdminDbn")]
    public string? AdminDbn { get; set; }

    [Browsable(true)]
    [Display(Name = "GDistrict")]
    public string? GDistrict { get; set; }

    [Browsable(true)]
    [Display(Name = "Borough")]
    public string? Borough { get; set; }

    [Browsable(true)]
    [Display(Name = "MandateShort")]
    public string? MandateShort { get; set; }

    [Browsable(true)]
    [Display(Name = "MandateStart")]
    public DateTime? MandateStart { get; set; }

    [Browsable(true)]
    [Display(Name = "MandatedMaxGroup")]
    public string? MandatedMaxGroup { get; set; }

    [Browsable(true)]
    [Display(Name = "AssignmentFirstEncounter")]
    public DateTime? AssignmentFirstEncounter { get; set; }

    [Browsable(true)]
    [Display(Name = "AssignmentClaimed")]
    public string? AssignmentClaimed { get; set; }

    [Browsable(true)]
    [Display(Name = "DateOfService")]
    public DateTime? DateOfService { get; set; }

    [Browsable(true)]
    [Display(Name = "ServiceType")]
    public string? ServiceType { get; set; }

    [Browsable(true)]
    [Display(Name = "LanguageProvided")]
    public string? LanguageProvided { get; set; }

    [Browsable(true)]
    [Display(Name = "SessionType")]
    public string? SessionType { get; set; }

    [Browsable(true)]
    [Display(Name = "SessionNotes")]
    public string? SessionNotes { get; set; }

    [Browsable(true)]
    [Display(Name = "Grouping")]
    public string? Grouping { get; set; }

    [Browsable(true)]
    [Display(Name = "ActualSize")]
    public string? ActualSize { get; set; }

    [Browsable(true)]
    [Display(Name = "StartTime")]
    public string? StartTime { get; set; }

    [Browsable(true)]
    [Display(Name = "EndTime")]
    public string? EndTime { get; set; }

    [Browsable(true)]
    [Display(Name = "Duration")]
    public string? Duration { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderLastName")]
    public string? ProviderLastName { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderFirstName")]
    public string? ProviderFirstName { get; set; }

    [Browsable(true)]
    [Display(Name = "FileName")]
    public string? FileName { get; set; }

    [Browsable(true)]
    [Display(Name = "RowNumber")]
    public int? RowNumber { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderId")]
    public int? ProviderId { get; set; }

    [Browsable(true)]
    [Display(Name = "EntryId")]
    public int? EntryId { get; set; }

    [Browsable(true)]
    [Display(Name = "BilledRate")]
    public decimal? BilledRate { get; set; }

    [Browsable(true)]
    [Display(Name = "BilledAmount")]
    public decimal? BilledAmount { get; set; }

    [Browsable(true)]
    [Display(Name = "BilledDate")]
    public DateTime? BilledDate { get; set; }

    [Browsable(true)]
    [Display(Name = "BilledPaidDate")]
    public DateTime? BilledPaidDate { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderRate")]
    public decimal? ProviderRate { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderAmount")]
    public decimal? ProviderAmount { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderPaidDate")]
    public DateTime? ProviderPaidDate { get; set; }

    [Browsable(true)]
    [Display(Name = "IsOverlap")]
    public bool IsOverlap { get; set; }

    [Browsable(true)]
    [Display(Name = "Voucher")]
    public string? Voucher { get; set; }

    [Browsable(true)]
    [Display(Name = "IsOverMandate")]
    public bool IsOverMandate { get; set; }

    [Browsable(true)]
    [Display(Name = "IsOverDuration")]
    public bool IsOverDuration { get; set; }

    [Browsable(true)]
    [Display(Name = "IsUnderGroup")]
    public bool IsUnderGroup { get; set; }
}
