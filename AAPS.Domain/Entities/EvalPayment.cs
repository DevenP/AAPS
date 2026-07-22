using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

// One row per line from an Evaluation Voucher Payment file (#9): the payment/deduction trail
// for an eval. The eval's running paid total lives on Eval.VoucherAmount; this is the history
// of each payment, deduction and repeat payment that made up that total.
[Table("EvalPayment")]
public partial class EvalPayment
{
    [Key]
    public int EvalPayment_Id { get; set; }

    public int? Eval_Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Voucher { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? ServiceSubtype { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentDate { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal? Amount { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? FileName { get; set; }

    public int? RowNumber { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }
}
