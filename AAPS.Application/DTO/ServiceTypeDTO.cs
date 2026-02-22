using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record ServiceTypeDTO
{
    [Browsable(true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Browsable(true)]
    [Display(Name = "IsEvaluation")]
    public bool IsEvaluation { get; set; }
}
