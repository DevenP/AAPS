using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ProviderRateDTO
{
    [DisplayField("Id")]
    public int Id { get; set; }

    [DisplayField("Provider ID")]
    public int? ProviderId { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("District")]
    public string? District { get; set; }

    [DisplayField("Rate")]
    public decimal? Rate { get; set; }

    [DisplayField("Effective Date")]
    public DateTime? EffectiveDate { get; set; }

    [DisplayField("Is Active")]
    public bool IsActive { get; set; }

    [DisplayField("Language")]
    public string? Language { get; set; }

    [DisplayField("Provider First Name")]
    public string? ProviderFirstName { get; set; }

    [DisplayField("Provider Last Name")]
    public string? ProviderLastName { get; set; }
}
