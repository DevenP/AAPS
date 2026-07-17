using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

// Line-by-line adjustment ledger for a session (#6/#20): write-offs (loss), funds returned
// to the DOE, and manual +/- adjustments. Payments themselves live in the Payment table;
// this table is the trail of everything that happens after billing.
[Table("PaymentTransaction")]
public partial class PaymentTransaction
{
    [Key]
    public int Transaction_Id { get; set; }

    public int Sesis_Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime TxnDate { get; set; }

    // "Write-Off" | "Returned to DOE" | "Adjustment"
    [StringLength(30)]
    [Unicode(false)]
    public string TxnType { get; set; } = null!;

    [Column(TypeName = "decimal(9, 2)")]
    public decimal Amount { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? Note { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }
}
