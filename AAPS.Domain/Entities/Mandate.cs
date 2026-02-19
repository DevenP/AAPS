using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

[Index("Entry_Id", Name = "_dta_index_Mandates_5_514100872__K1_15_18_24_25")]
[Index("Entry_Id", Name = "_dta_index_Mandates_5_514100872__K1_24")]
[Index("Entry_Id", "Grp_Size", "Student_ID", "MandateStart", "Service_Type", Name = "_dta_index_Mandates_5_514100872__K1_K15_K3_K24_K13_16_18")]
[Index("Entry_Id", "MandateEnd", Name = "_dta_index_Mandates_5_514100872__K1_K25_24")]
[Index("Student_ID", "Service_Type", Name = "_dta_index_Mandates_5_514100872__K3_K13_1_2_4_5_6_7_8_9_10_11_12_14_15_16_17_18_19_20_21_22_23_24_25_26_27_32")]
[Index("Student_ID", "Last_Name", "First_Name", Name = "_dta_index_Mandates_5_514100872__K3_K4_K5")]
public partial class Mandate
{
    [Key]
    public int Entry_Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Conf_Date { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Student_ID { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Last_Name { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? First_Name { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? Home_District { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? CSE { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? CSE_District { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Grade { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Date_of_Birth { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Admin_DBN { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? D75 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Service_Type { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Lang { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? Grp_Size { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Dur { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? Service_Location { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Remaining_Freq { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Provider { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? First_Attend_Date { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Mandate_ID { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Primary_Contact_Phone_1 { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Primary_Contact_Phone_2 { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? MandateStart { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? MandateEnd { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? FileName { get; set; }

    public int? RowNumber { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Service_Start_Date { get; set; }
}
