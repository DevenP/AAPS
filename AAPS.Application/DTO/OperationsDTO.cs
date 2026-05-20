using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO
{
    public record OperationsDTO : SesiDTO
    {
        [DisplayField("Mismatched Approval", GroupName = "Alerts")]
        public bool MandateFlag { get; set; }

        [DisplayField("Unassigned Provider", GroupName = "Alerts")]
        public bool ProviderFlag { get; set; }

        [DisplayField("Missing Billing Rate", GroupName = "Alerts")]
        public bool BRateFlag { get; set; }

        [DisplayField("Missing Provider Rate", GroupName = "Alerts")]
        public bool PRateFlag { get; set; }

        [DisplayField("Missing Assignment", GroupName = "Alerts")]
        public bool AssignFlag { get; set; }

        [DisplayField("Assign ID", GroupName = "Billing")]
        public string? AssignId { get; set; }

        [DisplayField("Full Address", browsable: false)]
        public string? FullAddress { get; set; }

        [DisplayField("SSN", browsable: false)]
        public string? Ssn { get; set; }
    }
}
