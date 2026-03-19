using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record EvalDTO
{
    [DisplayField("Id")]
    public int Id { get; set; }

    [DisplayField("First Name")]
    public string? StudentFirstName { get; set; }

    [DisplayField("Last Name")]
    public string? StudentLastName { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Phone")]
    public string? Phone { get; set; }

    [DisplayField("Email")]
    public string? Email { get; set; }

    [DisplayField("Parent First")]
    public string? ParentFirstName { get; set; }

    [DisplayField("Parent Last")]
    public string? ParentLastName { get; set; }

    [DisplayField("Eval Received")]
    public DateTime? EvalReceivedDate { get; set; }

    [DisplayField("Provider ID", browsable: false)]
    public int? ProviderId { get; set; }

    [DisplayField("Assigned Date")]
    public DateTime? AssignedDate { get; set; }

    [DisplayField("Report Received")]
    public DateTime? ReportReceivedDate { get; set; }

    [DisplayField("Eval Date")]
    public DateTime? EvalDate { get; set; }

    [DisplayField("Report Submitted")]
    public DateTime? ReportSubmittedDate { get; set; }

    [DisplayField("District")]
    public string? District { get; set; }

    [DisplayField("Language")]
    public string? Language { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Contact")]
    public string? Contact { get; set; }

    [DisplayField("Provider Paid Amt")]
    public decimal? ProviderPaidAmount { get; set; }

    [DisplayField("Billing Amount")]
    public decimal? BillingAmount { get; set; }

    [DisplayField("Provider Paid Date")]
    public DateTime? ProviderPaidDate { get; set; }

    [DisplayField("Bill Paid Date")]
    public DateTime? BillPaidDate { get; set; }

    [DisplayField("Billed Date")]
    public DateTime? BilledDate { get; set; }

    [DisplayField("Memo")]
    public string? Memo { get; set; }

    [DisplayField("Appointment Date")]
    public DateTime? AppointmentDate { get; set; }

    [DisplayField("Status")]
    public string? Status { get; set; }

    [DisplayField("Provider First")]
    public string? ProviderFirstName { get; set; }

    [DisplayField("Provider Last")]
    public string? ProviderLastName { get; set; }
}
