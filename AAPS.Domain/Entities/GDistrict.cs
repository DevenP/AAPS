using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

public partial class GDistrict
{
    [Key]
    public int Dist_Id { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? GDist { get; set; }
}
