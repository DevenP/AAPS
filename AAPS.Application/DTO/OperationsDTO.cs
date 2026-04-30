using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO
{
    public record OperationsDTO : SesiDTO
    {
        [DisplayField("Mandate Flag", browsable: false)]
        public bool MandateFlag { get; set; }

        [DisplayField("Provider Flag", browsable: false)]
        public bool ProviderFlag { get; set; }

        [DisplayField("Billed Rate Flag", browsable: false)]
        public bool BRateFlag { get; set; }

        [DisplayField("Provider Rate Flag", browsable: false)]
        public bool PRateFlag { get; set; }

        [DisplayField("Assign Flag", browsable: false)]
        public bool AssignFlag { get; set; }

        [DisplayField("Assign ID")]
        public string? AssignId { get; set; }

        [DisplayField("Full Address", browsable: false)]
        public string? FullAddress { get; set; }

        [DisplayField("SSN", browsable: false)]
        public string? Ssn { get; set; }
    }
}
