using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record EvalDTO
{
    [DisplayField("Id", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("Student Last Name", GroupName = "Student")]
    public string? StudentLastName { get; set; }

    [DisplayField("Student First Name", GroupName = "Student")]
    public string? StudentFirstName { get; set; }

    [DisplayField("Student ID", GroupName = "Student")]
    public string? StudentId { get; set; }

    [DisplayField("District", GroupName = "Student")]
    public string? District { get; set; }

    [DisplayField("Service Type", GroupName = "Service")]
    public string? ServiceType { get; set; }

    [DisplayField("Language", GroupName = "Service")]
    public string? Language { get; set; }

    [DisplayField("Phone", GroupName = "Contact")]
    public string? Phone { get; set; }

    [DisplayField("Email", browsable: false)]
    public string? Email { get; set; }

    [DisplayField("Parent Last Name", GroupName = "Contact")]
    public string? ParentLastName { get; set; }

    [DisplayField("Parent First Name", GroupName = "Contact")]
    public string? ParentFirstName { get; set; }

    [DisplayField("Referral Received", GroupName = "Dates")]
    public DateTime? EvalReceivedDate { get; set; }

    [DisplayField("Provider ID", browsable: false)]
    public int? ProviderId { get; set; }

    [DisplayField("Provider First Name", browsable: false)]
    public string? ProviderFirstName { get; set; }

    [DisplayField("Provider", GroupName = "Provider")]
    public string? ProviderLastName { get; set; }

    [DisplayField("Unassigned Provider", GroupName = "Provider")]
    public bool IsUnassigned { get; set; }

    [DisplayField("Assigned On", GroupName = "Provider")]
    public DateTime? AssignedDate { get; set; }

    [DisplayField("Appointment", GroupName = "Dates")]
    public DateTime? AppointmentDate { get; set; }

    [DisplayField("Report Received", GroupName = "Dates")]
    public DateTime? ReportReceivedDate { get; set; }

    [DisplayField("Evaluation Date", GroupName = "Dates")]
    public DateTime? EvalDate { get; set; }

    [DisplayField("Report Submitted", GroupName = "Dates")]
    public DateTime? ReportSubmittedDate { get; set; }

    [DisplayField("Status", GroupName = "Dates")]
    public string? Status { get; set; }

    [DisplayField("DoE Contact", GroupName = "Contact")]
    public string? Contact { get; set; }

    [DisplayField("Provider Paid Amt", browsable: false)]
    public decimal? ProviderPaidAmount { get; set; }

    [DisplayField("Billing Amount", browsable: false)]
    public decimal? BillingAmount { get; set; }

    [DisplayField("Billed Date", GroupName = "Dates")]
    public DateTime? BilledDate { get; set; }

    [DisplayField("Bill Paid Date", GroupName = "Dates")]
    public DateTime? BillPaidDate { get; set; }

    [DisplayField("Provider Paid Date", GroupName = "Dates")]
    public DateTime? ProviderPaidDate { get; set; }

    [DisplayField("Memo", browsable: false)]
    public string? Memo { get; set; }
}
