using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public class BillingRecordDTO
{
    [DisplayField("Sesis ID")]
    public int SesisId { get; set; }

    [DisplayField("Service Date")]
    public DateTime? DateOfService { get; set; }

    [DisplayField("Start Time")]
    public string? StartTime { get; set; }

    [DisplayField("End Time")]
    public string? EndTime { get; set; }

    [DisplayField("Provider")]
    public string? Provider { get; set; }

    [DisplayField("G. District")]
    public string? GDistrict { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Student")]
    public string? Student { get; set; }

    [DisplayField("Grade")]
    public string? Grade { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Group")]
    public string? ActualSize { get; set; }

    [DisplayField("Duration")]
    public string? Duration { get; set; }

    [DisplayField("Frequency")]
    public string? Frequency { get; set; }

    [DisplayField("Billed On")]
    public DateTime? Billed { get; set; }

    [DisplayField("Bill Paid On")]
    public DateTime? BilledPaidOn { get; set; }

    [DisplayField("Provider Paid On")]
    public DateTime? ProviderPaidOn { get; set; }

    [DisplayField("Billing Rate")]
    public decimal? BillingRate { get; set; }

    [DisplayField("Provider Rate")]
    public decimal? ProviderRate { get; set; }

    [DisplayField("Billing Amount")]
    public decimal? BillingAmount { get; set; }

    [DisplayField("Provider Amount")]
    public decimal? ProviderAmount { get; set; }

    [DisplayField("Assignment ID")]
    public string? AssignId { get; set; }

    [DisplayField("Approval ID")]
    public int? EntryId { get; set; }
}
