using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO
{
    public record OperationsDTO : SesiDTO
    {
        [DisplayField("Mandate Flag")]
        public bool MandateFlag { get; set; }

        [DisplayField("Provider Flag")]
        public bool ProviderFlag { get; set; }

        [DisplayField("Billed Rate Flag")]
        public bool BRateFlag { get; set; }

        [DisplayField("Provider Rate Flag")]
        public bool PRateFlag { get; set; }

        [DisplayField("Assign Flag")]
        public bool AssignFlag { get; set; }

        [DisplayField("Assign ID")]
        public string? AssignId { get; set; }

        [DisplayField("Full Address")]
        public string? FullAddress { get; set; }

        [DisplayField("SSN")]
        public string? Ssn { get; set; }
    }
}
