using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Domain.Entities;

[Table("Sesis")]
[Index("Student_ID", "Service_Type", "Provider_Last_Name", Name = "_dta_index_Sesis_5_1090102924__K2_K19_K28_1_3_4_5_6_7_8_9_10_11_12_13_14_15_16_17_18_20_21_22_23_24_25_26_27_29_30_31_32_33_34_")]
[Index("Provider_Id", "Entry_Id", Name = "_dta_index_Sesis_5_1090102924__K32_K33_1")]
[Index("Provider_Id", "Entry_Id", "Sesis_Id", Name = "_dta_index_Sesis_5_1090102924__K32_K33_K1_2")]
[Index("Entry_Id", "Provider_Id", "date_of_Service", "bRate", "pRate", "Overlap", "OverMandate", "OverDuration", "Billed", "Sesis_Id", "Student_ID", "Service_Type", "Last_Name", "Provider_Last_Name", "pPaid", "Provider_First_Name", Name = "_dta_index_Sesis_5_1090102924__K33_K32_K18_K34_K38_K41_K43_K46_K36_K1_K2_K19_K3_K28_K40_K29_4_5_6_7_8_9_10_11_12_13_14_15_16_")]
[Index("Overlap", "OverMandate", "OverDuration", "date_of_Service", "Billed", "pPaid", "bPaid", "Student_ID", "Last_Name", "Provider_Last_Name", "Provider_Id", "Entry_Id", "Provider_First_Name", Name = "_dta_index_Sesis_5_1090102924__K41_K43_K46_K18_K36_K40_K37_K2_K3_K28_K32_K33_K29_1_4_5_6_7_8_9_10_11_12_13_14_15_16_17_19_20_")]
public partial class Sesi
{
    [Key]
    public int Sesis_Id { get; set; }

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
    public string? Grade { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? date_of_Birth { get; set; }

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
    public string? Admin_DBN { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? GDistrict { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Borough { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Mandate_Short { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Mandatetime_Start { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Mandated_Max_Group { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Assignment_First_Encounter { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Assignment_Claimed { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? date_of_Service { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Service_Type { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Language_Provided { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Session_Type { get; set; }

    [Unicode(false)]
    public string? Session_Notes { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Groupin { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? Actual_Size { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? Start_Time { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? End_Time { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string? Duration { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Provider_Last_Name { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Provider_First_Name { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? FileName { get; set; }

    public int? RowNumber { get; set; }

    public int? Provider_Id { get; set; }

    public int? Entry_Id { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? bRate { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? bAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Billed { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? bPaid { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? pRate { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? pAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? pPaid { get; set; }

    public bool? Overlap { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Voucher { get; set; }

    public bool? OverMandate { get; set; }

    public bool? OverDuration { get; set; }

    public bool? UnderGroup { get; set; }
}
