using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO
{
    public record VendorPortalDTO
    {
        [Browsable(true)]
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Browsable(true)]
        [Display(Name = "Provider SSN")]
        public string? ProviderSSN { get; set; }

        [Browsable(true)]
        [Display(Name = "Boro")]
        public string? Boro { get; set; }

        [Browsable(true)]
        [Display(Name = "District")]
        public string? District { get; set; }

        [Browsable(true)]
        [Display(Name = "School")]
        public string? School { get; set; }

        [Browsable(true)]
        [Display(Name = "Fund")]
        public string? Fund { get; set; }

        [Browsable(true)]
        [Display(Name = "StudentId")]
        public string? StudentId { get; set; }

        [Browsable(true)]
        [Display(Name = "Duration")]
        public string? Duration { get; set; }

        [Browsable(true)]
        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

        [Browsable(true)]
        [Display(Name = "GroupSize")]
        public string? GroupSize { get; set; }

        [Browsable(true)]
        [Display(Name = "ApprovalStartDate")]
        public DateTime? ApprovalStartDate { get; set; }

        [Browsable(true)]
        [Display(Name = "AssignmentId")]
        public string? AssignmentId { get; set; }

        [Browsable(true)]
        [Display(Name = "FileName")]
        public string? VenderPortalFile { get; set; }

        [Browsable(true)]
        [Display(Name = "EntryId")]
        public int? EntryId { get; set; }

        [Browsable(true)]
        [Display(Name = "StudentFirstName")]
        public string? StudentFirstName { get; set; }

        [Browsable(true)]
        [Display(Name = "StudentLastName")]
        public string? StudentLastName { get; set; }

        [Browsable(true)]
        [Display(Name = "ProviderFirstName")]
        public string? ProviderFirstName { get; set; }

        [Browsable(true)]
        [Display(Name = "ProviderLastName")]
        public string? ProviderLastName { get; set; }

        [Browsable(true)]
        [Display(Name = "Mismatch")]
        public string? Mismatch { get; set; } // 'Approval' or 'Provider'

        [Browsable(false)]
        [Display(Name = "Mismatched Vendor Portal")]
        public bool MismatchedVendorPortal { get; set; } // true when Entry_Id IS NULL (unlinked VP record)
    }
}