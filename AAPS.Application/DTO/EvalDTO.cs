using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record EvalDTO
{
    [DisplayField("Id", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("Student Last Name")]
    public string? StudentLastName { get; set; }

    [DisplayField("Student First Name")]
    public string? StudentFirstName { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("District")]
    public string? District { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Language")]
    public string? Language { get; set; }

    [DisplayField("Phone")]
    public string? Phone { get; set; }

    [DisplayField("Email", browsable: false)]
    public string? Email { get; set; }

    [DisplayField("Parent Last Name")]
    public string? ParentLastName { get; set; }

    [DisplayField("Parent First Name")]
    public string? ParentFirstName { get; set; }

    [DisplayField("Referral Received")]
    public DateTime? EvalReceivedDate { get; set; }

    [DisplayField("Provider ID", browsable: false)]
    public int? ProviderId { get; set; }

    [DisplayField("Provider First Name", browsable: false)]
    public string? ProviderFirstName { get; set; }

    [DisplayField("Provider")]
    public string? ProviderLastName { get; set; }

    [DisplayField("Unassigned Provider")]
    public bool IsUnassigned { get; set; }

    [DisplayField("Assigned On")]
    public DateTime? AssignedDate { get; set; }

    [DisplayField("Appointment")]
    public DateTime? AppointmentDate { get; set; }

    [DisplayField("Report Received")]
    public DateTime? ReportReceivedDate { get; set; }

    [DisplayField("Evaluation Date")]
    public DateTime? EvalDate { get; set; }

    [DisplayField("Report Submitted")]
    public DateTime? ReportSubmittedDate { get; set; }

    [DisplayField("Status")]
    public string? Status { get; set; }

    [DisplayField("DoE Contact")]
    public string? Contact { get; set; }

    [DisplayField("Provider Paid Amt", browsable: false)]
    public decimal? ProviderPaidAmount { get; set; }

    [DisplayField("Billing Amount", browsable: false)]
    public decimal? BillingAmount { get; set; }

    [DisplayField("Billed Date")]
    public DateTime? BilledDate { get; set; }

    [DisplayField("Bill Paid Date")]
    public DateTime? BillPaidDate { get; set; }

    [DisplayField("Provider Paid Date")]
    public DateTime? ProviderPaidDate { get; set; }

    [DisplayField("Memo", browsable: false)]
    public string? Memo { get; set; }
}
