using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

[Table("SystemSettings")]
public partial class SystemSetting
{
    [Key]
    [StringLength(100)]
    [Unicode(false)]
    public string SettingKey { get; set; } = null!;

    [StringLength(500)]
    public string? SettingValue { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
