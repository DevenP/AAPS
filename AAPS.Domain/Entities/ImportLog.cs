using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

[Table("ImportLog")]
public partial class ImportLog
{
    [Key]
    public int Log_Id { get; set; }

    [Unicode(false)]
    public string? ImportRecord { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ImportOn { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? FileName { get; set; }
}
