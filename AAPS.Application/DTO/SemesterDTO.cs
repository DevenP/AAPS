using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record SemesterDTO
{
    [DisplayField("Id")]
    public int Id { get; set; }

    [DisplayField("Code")]
    public string Code { get; set; } = "";

    [DisplayField("Start Date")]
    public DateTime StartDate { get; set; }

    [DisplayField("End Date")]
    public DateTime EndDate { get; set; }
}
