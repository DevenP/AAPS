using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record ImportLogDTO
{
    [Browsable(false)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "Import Record")]
    public string? ImportRecord { get; set; }

    [Browsable(true)]
    [Display(Name = "File Name")]
    public string? FileName { get; set; }

    [Browsable(true)]
    [Display(Name = "Import Date")]
    public DateTime? ImportDate { get; set; }
}
