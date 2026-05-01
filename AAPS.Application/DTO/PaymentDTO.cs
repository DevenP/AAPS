using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record PaymentDTO
{
    [DisplayField("ID", browsable: false, IsReadOnly = true)]
    public int VoucherId { get; set; }

    [DisplayField("Voucher")]
    public string? Voucher { get; set; }

    [DisplayField("Provider")]
    public string? Provider { get; set; }

    [DisplayField("Service Date")]
    public DateTime? DateOfService { get; set; }

    [DisplayField("Start Time")]
    public string? StartTime { get; set; }

    [DisplayField("End Time")]
    public string? EndTime { get; set; }

    [DisplayField("School")]
    public string? AdminDbn { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Billed Amount")]
    public decimal? BilledAmount { get; set; }

    [DisplayField("Billed On")]
    public DateTime? BilledOn { get; set; }

    [DisplayField("SSN", browsable: false)]
    public string? Ssn { get; set; }

    [DisplayField("Voucher Amount", browsable: false)]
    public decimal? VoucherAmount { get; set; }

    [DisplayField("File Name", browsable: false)]
    public string? FileName { get; set; }

    [DisplayField("Row #", browsable: false)]
    public int? RowNumber { get; set; }

    [DisplayField("Sesis ID", browsable: false)]
    public int? SesisId { get; set; }
}
