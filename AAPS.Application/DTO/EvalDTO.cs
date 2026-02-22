using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record EvalDTO
{
    [Browsable(true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "StudentFirstName")]
    public string? StudentFirstName { get; set; }

    [Browsable(true)]
    [Display(Name = "StudentLastName")]
    public string? StudentLastName { get; set; }

    [Browsable(true)]
    [Display(Name = "StudentId")]
    public string? StudentId { get; set; }

    [Browsable(true)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Browsable(true)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Browsable(true)]
    [Display(Name = "ParentFirstName")]
    public string? ParentFirstName { get; set; }

    [Browsable(true)]
    [Display(Name = "ParentLastName")]
    public string? ParentLastName { get; set; }

    [Browsable(true)]
    [Display(Name = "EvalReceivedDate")]
    public DateTime? EvalReceivedDate { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderId")]
    public int? ProviderId { get; set; }

    [Browsable(true)]
    [Display(Name = "AssignedDate")]
    public DateTime? AssignedDate { get; set; }

    [Browsable(true)]
    [Display(Name = "ReportReceivedDate")]
    public DateTime? ReportReceivedDate { get; set; }

    [Browsable(true)]
    [Display(Name = "EvalDate")]
    public DateTime? EvalDate { get; set; }

    [Browsable(true)]
    [Display(Name = "ReportSubmittedDate")]
    public DateTime? ReportSubmittedDate { get; set; }

    [Browsable(true)]
    [Display(Name = "District")]
    public string? District { get; set; }

    [Browsable(true)]
    [Display(Name = "Language")]
    public string? Language { get; set; }

    [Browsable(true)]
    [Display(Name = "ServiceType")]
    public string? ServiceType { get; set; }

    [Browsable(true)]
    [Display(Name = "Contact")]
    public string? Contact { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderPaidAmount")]
    public decimal? ProviderPaidAmount { get; set; }

    [Browsable(true)]
    [Display(Name = "BillingAmount")]
    public decimal? BillingAmount { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderPaidDate")]
    public DateTime? ProviderPaidDate { get; set; }

    [Browsable(true)]
    [Display(Name = "BillPaidDate")]
    public DateTime? BillPaidDate { get; set; }

    [Browsable(true)]
    [Display(Name = "BilledDate")]
    public DateTime? BilledDate { get; set; }

    [Browsable(true)]
    [Display(Name = "Memo")]
    public string? Memo { get; set; }

    [Browsable(true)]
    [Display(Name = "AppointmentDate")]
    public DateTime? AppointmentDate { get; set; }

    [Browsable(true)]
    [Display(Name = "Status")]
    public string? Status { get; set; }

    //

    [Browsable(true)]
    [Display(Name = "ProviderFirstName")]
    public string? ProviderFirstName { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderLastName")]
    public string? ProviderLastName { get; set; }
}
