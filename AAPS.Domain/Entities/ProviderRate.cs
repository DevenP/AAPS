using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

[Table("ProviderRate")]
public partial class ProviderRate
{
    [Key]
    public int ProviderRate_Id { get; set; }

    public int? Provider_Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ServiceType { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? District { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? Rate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Effective { get; set; }

    public bool? Active { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Lang { get; set; }
}
