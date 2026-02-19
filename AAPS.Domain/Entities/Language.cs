using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

public partial class Language
{
    [Key]
    public int Language_Id { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Lang { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? LangCode { get; set; }
}
