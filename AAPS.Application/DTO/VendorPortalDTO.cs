using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO
{
    public record VendorPortalDTO
    {
        [DisplayField("Id", browsable: false, IsReadOnly = true)]
        public int Id { get; set; }

        [DisplayField("Mismatch", GroupName = "Approval")]
        [FilterOptions("Approval", "Provider")]
        public string? Mismatch { get; set; }

        [DisplayField("Student ID", GroupName = "Student")]
        public string? StudentId { get; set; }

        [DisplayField("Student Last Name", GroupName = "Student")]
        public string? StudentLastName { get; set; }

        [DisplayField("Student First Name", GroupName = "Student")]
        public string? StudentFirstName { get; set; }

        [DisplayField("Provider Last Name", GroupName = "Provider")]
        public string? ProviderLastName { get; set; }

        [DisplayField("Provider First Name", GroupName = "Provider")]
        public string? ProviderFirstName { get; set; }

        [DisplayField("Provider SSN", GroupName = "Provider")]
        public string? ProviderSSN { get; set; }

        [DisplayField("Duration", GroupName = "Service")]
        public string? Duration { get; set; }

        [DisplayField("Frequency", GroupName = "Service")]
        public string? Frequency { get; set; }

        [DisplayField("Group Size", GroupName = "Service")]
        public string? GroupSize { get; set; }

        [DisplayField("School", GroupName = "Service")]
        public string? School { get; set; }

        [DisplayField("Approval Start Date", GroupName = "Approval")]
        public DateTime? ApprovalStartDate { get; set; }

        [DisplayField("Assignment ID", GroupName = "Approval")]
        public string? AssignmentId { get; set; }

        [DisplayField("Fund", GroupName = "Approval")]
        public string? Fund { get; set; }

        [DisplayField("Vendor Portal File", GroupName = "Approval")]
        public string? VenderPortalFile { get; set; }

        [DisplayField("Approval ID", GroupName = "Approval")]
        public int? EntryId { get; set; }

        [DisplayField("Boro", browsable: false)]
        public string? Boro { get; set; }

        [DisplayField("District", browsable: false)]
        public string? District { get; set; }

        [DisplayField("Mismatched Vendor Portal", browsable: false)]
        public bool MismatchedVendorPortal { get; set; }
    }
}
