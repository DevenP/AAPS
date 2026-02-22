using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record ProviderContactDTO
{

    [ReadOnly(true)]
    [Browsable(false)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [ReadOnly(false)]
    [Browsable(true)]
    [Display(Name = "ProviderId")]
    public int? ProviderId { get; set; }

    [ReadOnly(true)]
    [Browsable(true)]
    [Display(Name = "ContactDate")]
    public DateTime? ContactDate { get; set; }

    [ReadOnly(false)]
    [Browsable(true)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}
