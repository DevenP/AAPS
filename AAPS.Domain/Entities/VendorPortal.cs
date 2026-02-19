using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

[Table("VendorPortal")]
[Index("pGrpSize", "pSsn", "Student_ID", "Entry_Id", "Assign_Id", "VPFile", Name = "_dta_index_VendorPortal_5_299148111__K10_K2_K7_K14_K12_K13_1_3_4_5_6_8_9_11")]
[Index("Assign_Id", "Entry_Id", Name = "_dta_index_VendorPortal_5_299148111__K12_K14")]
[Index("Entry_Id", "pSsn", "VendorPortal_Id", Name = "_dta_index_VendorPortal_5_299148111__K14_K2_K1")]
[Index("VendorPortal_Id", Name = "_dta_index_VendorPortal_5_299148111__K1_12")]
[Index("pSsn", Name = "_dta_index_VendorPortal_5_299148111__K2_3_5_6_12_14")]
public partial class VendorPortal
{
    [Key]
    public int VendorPortal_Id { get; set; }

    [StringLength(9)]
    [Unicode(false)]
    public string? pSsn { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? pBoro { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? pDist { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? pSchool { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? pFund { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Student_ID { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? pDur { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? pFreq { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? pGrpSize { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? pStartDate { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Assign_Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? VPFile { get; set; }

    public int? Entry_Id { get; set; }
}
