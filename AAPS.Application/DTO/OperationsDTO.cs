using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO
{
    public record OperationsDTO : SesiDTO
    {
        [DisplayField("Mismatched Approval")]
        public bool MandateFlag { get; set; }

        [DisplayField("Unassigned Provider")]
        public bool ProviderFlag { get; set; }

        [DisplayField("Missing Billing Rate")]
        public bool BRateFlag { get; set; }

        [DisplayField("Missing Provider Rate")]
        public bool PRateFlag { get; set; }

        [DisplayField("Missing Assignment")]
        public bool AssignFlag { get; set; }

        [DisplayField("Assign ID")]
        public string? AssignId { get; set; }

        [DisplayField("Full Address", browsable: false)]
        public string? FullAddress { get; set; }

        [DisplayField("SSN", browsable: false)]
        public string? Ssn { get; set; }
    }
}
