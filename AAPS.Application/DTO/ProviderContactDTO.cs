using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ProviderContactDTO
{
    [DisplayField("Id", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("Provider ID")]
    public int? ProviderId { get; set; }

    [DisplayField("Contact Date", IsReadOnly = true)]
    public DateTime? ContactDate { get; set; }

    [DisplayField("Notes")]
    public string? Notes { get; set; }
}
