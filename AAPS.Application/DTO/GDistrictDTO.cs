using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record GDistrictDTO
{
    [DisplayField("Id")]
    public int Id { get; set; }

    [DisplayField("District Code")]
    public string? DistrictCode { get; set; }
}
