using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public class BillingRecordDTO
{
    [DisplayField("Sesis ID", browsable: false)]
    public int SesisId { get; set; }

    [DisplayField("Service Date", GroupName = "Service")]
    public DateTime? DateOfService { get; set; }

    [DisplayField("Start Time", GroupName = "Service")]
    public string? StartTime { get; set; }

    [DisplayField("End Time", GroupName = "Service")]
    public string? EndTime { get; set; }

    [DisplayField("Provider", GroupName = "Provider")]
    public string? Provider { get; set; }

    [DisplayField("G. District", GroupName = "Student")]
    public string? GDistrict { get; set; }

    [DisplayField("Student ID", GroupName = "Student")]
    public string? StudentId { get; set; }

    [DisplayField("Student", GroupName = "Student")]
    public string? Student { get; set; }

    [DisplayField("Grade", GroupName = "Student")]
    public string? Grade { get; set; }

    [DisplayField("Service Type", GroupName = "Service")]
    public string? ServiceType { get; set; }

    [DisplayField("Group", GroupName = "Service")]
    public string? ActualSize { get; set; }

    [DisplayField("Duration", GroupName = "Service")]
    public string? Duration { get; set; }

    [DisplayField("Frequency", GroupName = "Service")]
    public string? Frequency { get; set; }

    [DisplayField("Billed On", GroupName = "Billing")]
    public DateTime? Billed { get; set; }

    [DisplayField("Bill Paid On", GroupName = "Billing")]
    public DateTime? BilledPaidOn { get; set; }

    [DisplayField("Provider Paid On", GroupName = "Billing")]
    public DateTime? ProviderPaidOn { get; set; }

    [DisplayField("Billing Rate", GroupName = "Billing")]
    public decimal? BillingRate { get; set; }

    [DisplayField("Provider Rate", GroupName = "Billing")]
    public decimal? ProviderRate { get; set; }

    [DisplayField("Billing Amount", GroupName = "Billing")]
    public decimal? BillingAmount { get; set; }

    [DisplayField("Provider Paid", GroupName = "Billing")]
    public decimal? ProviderAmount { get; set; }

    [DisplayField("Assignment ID", GroupName = "Billing")]
    public string? AssignId { get; set; }

    [DisplayField("Approval ID", GroupName = "Billing")]
    public int? EntryId { get; set; }
}
