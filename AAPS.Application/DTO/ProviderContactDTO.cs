using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record ProviderContactDTO
{
    [Browsable(true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "ProviderId")]
    public int? ProviderId { get; set; }

    [Browsable(true)]
    [Display(Name = "ContactDate")]
    public DateTime? ContactDate { get; set; }

    [Browsable(true)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}
