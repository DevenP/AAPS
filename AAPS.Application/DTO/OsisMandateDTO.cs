namespace AAPS.Application.DTO;

public class OsisMandateDTO
{
    public int EntryId { get; set; }
    public string? ServiceType { get; set; }
    public string? AdminDbn { get; set; }
    public string? Language { get; set; }
    public string? RemainingFrequency { get; set; }
    public string? Duration { get; set; }
    public string? GroupSize { get; set; }
    public DateTime? MandateStart { get; set; }
    public DateTime? MandateEnd { get; set; }
    /// <summary>Aggregated "LastName, FirstName: AssignId; ..." string (mirrors STRING_AGG in Mandates_By_Osis)</summary>
    public string? AssignIds { get; set; }
}
