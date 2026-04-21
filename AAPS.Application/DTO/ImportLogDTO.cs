using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record ImportLogDTO
{
    [DisplayField("Id", browsable: false)]
    public int Id { get; set; }

    [DisplayField("Import Record")]
    public string? ImportRecord { get; set; }

    [DisplayField("File Name")]
    public string? FileName { get; set; }

    [DisplayField("Import Date")]
    public DateTime? ImportDate { get; set; }
}
