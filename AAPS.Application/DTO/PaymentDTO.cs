using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record PaymentDTO
{
    [DisplayField("ID", browsable: false, IsReadOnly = true)]
    public int VoucherId { get; set; }

    [DisplayField("Voucher")]
    public string? Voucher { get; set; }

    [DisplayField("Invoice #")]
    public string? InvoiceNumber { get; set; }

    [DisplayField("Batch ID")]
    public string? BatchId { get; set; }

    [DisplayField("Subtype")]
    public string? ServiceSubtype { get; set; }

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

    [DisplayField("Paid Amount")]
    public decimal? VoucherAmount { get; set; }

    [DisplayField("IVR Confirm")]
    public string? IvrConfirm { get; set; }

    // Friendly expansion of the subtype code (e.g. "OT — Individual"); for display/tooltip only.
    [DisplayField("Subtype (detail)", browsable: false)]
    public string? ServiceSubtypeLabel => ServiceSubtype switch
    {
        null or "" => null,
        var s when s.Equals("O1", StringComparison.OrdinalIgnoreCase) => "Occupational Therapy — Individual",
        var s when s.Equals("OT", StringComparison.OrdinalIgnoreCase) => "Occupational Therapy — Group",
        var s when s.Equals("S1", StringComparison.OrdinalIgnoreCase) => "Speech — Individual",
        var s when s.Equals("SP", StringComparison.OrdinalIgnoreCase) => "Speech — Group",
        var s when s.Equals("P1", StringComparison.OrdinalIgnoreCase) => "Physical Therapy — Individual",
        var s when s.Equals("PT", StringComparison.OrdinalIgnoreCase) => "Physical Therapy — Group",
        var s when s.Equals("C1", StringComparison.OrdinalIgnoreCase) => "Counseling — Individual",
        var s when s.Equals("CO", StringComparison.OrdinalIgnoreCase) => "Counseling — Group",
        _ => ServiceSubtype
    };

    [DisplayField("SSN", browsable: false)]
    public string? Ssn { get; set; }

    [DisplayField("File Name", browsable: false)]
    public string? FileName { get; set; }

    [DisplayField("Row #", browsable: false)]
    public int? RowNumber { get; set; }

    [DisplayField("Sesis ID", browsable: false)]
    public int? SesisId { get; set; }
}
