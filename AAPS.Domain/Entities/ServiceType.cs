using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

public partial class ServiceType
{
    [Key]
    public int ServiceType_Id { get; set; }

    [Column("ServiceType")]
    [StringLength(50)]
    [Unicode(false)]
    public string? ServiceType1 { get; set; }

    public bool? Eval { get; set; }
}
