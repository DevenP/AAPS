using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ProviderBillingDTO
{
    [DisplayField("Id", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("Student ID", GroupName = "Student")]
    public string? StudentId { get; set; }

    [DisplayField("Student Name", GroupName = "Student")]
    public string? StudentLastName { get; set; }

    [DisplayField("Student First Name", browsable: false)]
    public string? StudentFirstName { get; set; }

    [DisplayField("Provider", GroupName = "Provider")]
    public string? ProviderLastName { get; set; }

    [DisplayField("Provider First Name", browsable: false)]
    public string? ProviderFirstName { get; set; }

    [DisplayField("Approval ID", GroupName = "Provider")]
    public int? EntryId { get; set; }

    [DisplayField("Service Date", GroupName = "Service")]
    public DateTime? DateOfService { get; set; }

    [DisplayField("Start Time", GroupName = "Service")]
    public string? StartTime { get; set; }

    [DisplayField("End Time", GroupName = "Service")]
    public string? EndTime { get; set; }

    [DisplayField("Duration", GroupName = "Service")]
    public string? Duration { get; set; }

    [DisplayField("Service Type", GroupName = "Service")]
    public string? ServiceType { get; set; }

    [DisplayField("G District", GroupName = "Student")]
    public string? GDistrict { get; set; }

    [DisplayField("Billed Rate", GroupName = "Billing")]
    public decimal? BilledRate { get; set; }

    [DisplayField("Billed Amount", GroupName = "Billing")]
    public decimal? BilledAmount { get; set; }

    [DisplayField("Billed Date", GroupName = "Billing")]
    public DateTime? BilledDate { get; set; }

    [DisplayField("Bill Paid Date", GroupName = "Billing")]
    public DateTime? BilledPaidDate { get; set; }

    [DisplayField("Provider Rate", GroupName = "Billing")]
    public decimal? ProviderRate { get; set; }

    [DisplayField("Provider Amount", GroupName = "Billing")]
    public decimal? ProviderAmount { get; set; }

    [DisplayField("Provider Paid Date", GroupName = "Billing")]
    public DateTime? ProviderPaidDate { get; set; }

    [DisplayField("Voucher", GroupName = "Billing")]
    public string? Voucher { get; set; }
}
