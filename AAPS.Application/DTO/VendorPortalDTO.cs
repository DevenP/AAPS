using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO
{
    public record VendorPortalDTO
    {
        [DisplayField("Id")]
        public int Id { get; set; }

        [DisplayField("Provider SSN")]
        public string? ProviderSSN { get; set; }

        [DisplayField("Boro")]
        public string? Boro { get; set; }

        [DisplayField("District")]
        public string? District { get; set; }

        [DisplayField("School")]
        public string? School { get; set; }

        [DisplayField("Fund")]
        public string? Fund { get; set; }

        [DisplayField("Student ID")]
        public string? StudentId { get; set; }

        [DisplayField("Duration")]
        public string? Duration { get; set; }

        [DisplayField("Frequency")]
        public string? Frequency { get; set; }

        [DisplayField("Group Size")]
        public string? GroupSize { get; set; }

        [DisplayField("Approval Start Date")]
        public DateTime? ApprovalStartDate { get; set; }

        [DisplayField("Assignment ID")]
        public string? AssignmentId { get; set; }

        [DisplayField("File Name")]
        public string? VenderPortalFile { get; set; }

        [DisplayField("Entry ID")]
        public int? EntryId { get; set; }

        [DisplayField("Student First Name")]
        public string? StudentFirstName { get; set; }

        [DisplayField("Student Last Name")]
        public string? StudentLastName { get; set; }

        [DisplayField("Provider First Name")]
        public string? ProviderFirstName { get; set; }

        [DisplayField("Provider Last Name")]
        public string? ProviderLastName { get; set; }

        [DisplayField("Mismatch")]
        [FilterOptions("Approval", "Provider")]
        public string? Mismatch { get; set; }

        [DisplayField("Mismatched Vendor Portal", browsable: false)]
        public bool MismatchedVendorPortal { get; set; }
    }
}
