using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record LanguageDTO
{
    [DisplayField("Id")]
    public int Id { get; set; }

    [DisplayField("Name")]
    public string? Name { get; set; }

    [DisplayField("Code")]
    public string? Code { get; set; }
}
