using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

[Table("Payment")]
public partial class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Voucher_Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Voucher { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? Student_ID { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Ssn { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Provider { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? date_of_Service { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Start_Time { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? VoucherAmount { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? FileName { get; set; }

    public int? RowNumber { get; set; }

    public int? Sesis_Id { get; set; }
}
