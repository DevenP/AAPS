using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ServiceTypeDTO
{
    [DisplayField("Id")]
    public int Id { get; set; }

    [DisplayField("Name")]
    public string? Name { get; set; }

    [DisplayField("Is Evaluation")]
    public bool IsEvaluation { get; set; }
}
