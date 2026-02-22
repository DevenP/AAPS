using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record LanguageDTO
{
    [Browsable(true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Browsable(true)]
    [Display(Name = "Code")]
    public string? Code { get; set; }
}
