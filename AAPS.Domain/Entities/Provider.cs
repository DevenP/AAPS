using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

public partial class Provider
{
    [Key]
    public int Provider_Id { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? Ssn { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? LastName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? FirstName { get; set; }

    [StringLength(12)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? TaxId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Birthdate { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? NpiNumber { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Liability { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? License1 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? License1Exp { get; set; }

    [StringLength(11)]
    [Unicode(false)]
    public string? License2 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? License2Exp { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Medical { get; set; }

    public bool? Pets { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? W9 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DirectDeposit { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Contract { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PhotoId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Resume { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? HrBundle { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ProofCorp { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Policies { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Medicaid { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SexualHarassment { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? CorpName { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ServiceType { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Address { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? City { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? State { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Zipcode { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? BlExt { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Langs { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? DDInfo { get; set; }
}
