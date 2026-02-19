namespace AAPS.Application.DTO;

public record ProviderContactDTO
{
    public int Id { get; init; }
    public int? ProviderId { get; init; }
    public DateTime? ContactDate { get; init; }
    public string? Notes { get; init; }
}
