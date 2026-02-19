namespace AAPS.Application.DTO;

public record ProviderRateDTO
{
    public int Id { get; init; }
    public int? ProviderId { get; init; }
    public string? ServiceType { get; init; }
    public string? District { get; init; }
    public decimal? Rate { get; init; }
    public DateTime? EffectiveDate { get; init; }
    public bool IsActive { get; init; }
    public string? Language { get; init; }
}
