using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ProviderBillingDTO
{
    [DisplayField("Id", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Student Name")]
    public string? StudentLastName { get; set; }

    [DisplayField("Student First Name", browsable: false)]
    public string? StudentFirstName { get; set; }

    [DisplayField("Provider")]
    public string? ProviderLastName { get; set; }

    [DisplayField("Provider First Name", browsable: false)]
    public string? ProviderFirstName { get; set; }

    [DisplayField("Approval ID")]
    public int? EntryId { get; set; }

    [DisplayField("Service Date")]
    public DateTime? DateOfService { get; set; }

    [DisplayField("Start Time")]
    public string? StartTime { get; set; }

    [DisplayField("End Time")]
    public string? EndTime { get; set; }

    [DisplayField("Duration")]
    public string? Duration { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("G District")]
    public string? GDistrict { get; set; }

    [DisplayField("Billed Rate")]
    public decimal? BilledRate { get; set; }

    [DisplayField("Billed Amount")]
    public decimal? BilledAmount { get; set; }

    [DisplayField("Billed Date")]
    public DateTime? BilledDate { get; set; }

    [DisplayField("Bill Paid Date")]
    public DateTime? BilledPaidDate { get; set; }

    [DisplayField("Provider Rate")]
    public decimal? ProviderRate { get; set; }

    [DisplayField("Provider Amount")]
    public decimal? ProviderAmount { get; set; }

    [DisplayField("Provider Paid Date")]
    public DateTime? ProviderPaidDate { get; set; }

    [DisplayField("Voucher")]
    public string? Voucher { get; set; }
}
