using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

public partial class Eval
{
    [Key]
    public int Eval_Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? StudentLast { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? StudentFirst { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? Student_ID { get; set; }

    [StringLength(12)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ParentLast { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ParentFirst { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EvalReceived { get; set; }

    public int? Provider_Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Assigned { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReportReceived { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EvalDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReportSubmitted { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? District { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Language { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ServiceType { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Contact { get; set; }

    [Column(TypeName = "decimal(7, 2)")]
    public decimal? pAmount { get; set; }

    [Column(TypeName = "decimal(7, 2)")]
    public decimal? bAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? pPaid { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? bPaid { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Billed { get; set; }

    [Unicode(false)]
    public string? Memo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Appointment { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }
}
