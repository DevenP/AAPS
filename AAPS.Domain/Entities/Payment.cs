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

    // Invoice number (file col B = PAYM_INVOICE_NUM)
    [StringLength(50)]
    [Unicode(false)]
    public string? InvoiceNumber { get; set; }

    // Batch ID (file col C = PCIB_BATCH_ID)
    [StringLength(50)]
    [Unicode(false)]
    public string? BatchId { get; set; }

    // Service subtype code (file col K = PAYM_SERV_SUBTYPE, e.g. O1 / OT)
    [StringLength(10)]
    [Unicode(false)]
    public string? ServiceSubtype { get; set; }

    // IVR confirmation (file col R = PAYM_IVR_CONFIRM)
    [StringLength(50)]
    [Unicode(false)]
    public string? IvrConfirm { get; set; }

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
