using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record PaymentDTO
{
    [DisplayField("ID")]
    public int VoucherId { get; set; }

    [DisplayField("Voucher")]
    public string? Voucher { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("SSN")]
    public string? Ssn { get; set; }

    [DisplayField("Provider")]
    public string? Provider { get; set; }

    [DisplayField("Date of Service")]
    public DateTime? DateOfService { get; set; }

    [DisplayField("Start Time")]
    public string? StartTime { get; set; }

    [DisplayField("Voucher Amount")]
    public decimal? VoucherAmount { get; set; }

    [DisplayField("File Name")]
    public string? FileName { get; set; }

    [DisplayField("Row #")]
    public int? RowNumber { get; set; }

    [DisplayField("Sesis ID")]
    public int? SesisId { get; set; }
}
