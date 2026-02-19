using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

[Table("Provider_Contact")]
public partial class Provider_Contact
{
    [Key]
    public int Contact_Id { get; set; }

    public int? Provider_Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ContactDate { get; set; }

    [Unicode(false)]
    public string? ContactNote { get; set; }
}
