using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record BillingRateDTO
{
    [Browsable(true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "District")]
    public string? District { get; set; }

    [Browsable(true)]
    [Display(Name = "ServiceType")]
    public string? ServiceType { get; set; }

    [Browsable(true)]
    [Display(Name = "Rate")]
    public decimal? Rate { get; set; }

    [Browsable(true)]
    [Display(Name = "EffectiveDate")]
    public DateTime? EffectiveDate { get; set; }

    [Browsable(true)]
    [Display(Name = "IsActive")]
    public bool IsActive { get; set; }

    [Browsable(true)]
    [Display(Name = "Language")]
    public string? Language { get; set; }
}
