using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO
{
    public record OperationsDTO : SesiDTO
    {
        [Browsable(true)]
        [Display(Name = "MandateFlag")]
        public bool MandateFlag { get; set; }

        [Browsable(true)]
        [Display(Name = "ProviderFlag")]
        public bool ProviderFlag { get; set; }

        [Browsable(true)]
        [Display(Name = "BRateFlag")]
        public bool BRateFlag { get; set; }

        [Browsable(true)]
        [Display(Name = "PRateFlag")]
        public bool PRateFlag { get; set; }

        [Browsable(true)]
        [Display(Name = "AssignFlag")]
        public bool AssignFlag { get; set; }

        //

        [Browsable(true)]
        [Display(Name = "AssignId")]
        public string? AssignId { get; set; }

        [Browsable(true)]
        [Display(Name = "FullAddress")]
        public string? FullAddress { get; set; }

        [Browsable(true)]
        [Display(Name = "Ssn")]
        public string? Ssn { get; set; }
    }
}
