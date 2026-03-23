using System.ComponentModel.DataAnnotations;
using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record SesiDTO
{
    [DisplayField("Id")]
    public int Id { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Student Last Name")]
    public string? StudentLastName { get; set; }

    [DisplayField("Student First Name")]
    public string? StudentFirstName { get; set; }

    [DisplayField("Grade")]
    public string? Grade { get; set; }

    [DisplayField("Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [DisplayField("Home District")]
    public string? HomeDistrict { get; set; }

    [DisplayField("CSE")]
    public string? Cse { get; set; }

    [DisplayField("CSE District")]
    public string? CseDistrict { get; set; }

    [DisplayField("Admin DBN")]
    public string? AdminDbn { get; set; }

    [DisplayField("G District")]
    public string? GDistrict { get; set; }

    [DisplayField("Borough")]
    public string? Borough { get; set; }

    [DisplayField("Mandate Short")]
    public string? MandateShort { get; set; }

    [DisplayField("Mandate Start")]
    public DateTime? MandateStart { get; set; }

    [DisplayField("Mandate End")]
    public DateTime? MandateEnd { get; set; }

    [DisplayField("Mandated Max Group")]
    public string? MandatedMaxGroup { get; set; }

    [DisplayField("Assignment First Encounter")]
    public DateTime? AssignmentFirstEncounter { get; set; }

    [DisplayField("Assignment Claimed")]
    public string? AssignmentClaimed { get; set; }

    [DisplayField("Date of Service")]
    public DateTime? DateOfService { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Language Provided")]
    public string? LanguageProvided { get; set; }

    [DisplayField("Session Type")]
    public string? SessionType { get; set; }

    [DisplayField("Session Notes")]
    public string? SessionNotes { get; set; }

    [DisplayField("Grouping")]
    public string? Grouping { get; set; }

    [DisplayField("Actual Size")]
    public string? ActualSize { get; set; }

    [DisplayField("Start Time")]
    public string? StartTime { get; set; }

    [DisplayField("End Time")]
    public string? EndTime { get; set; }

    [DisplayField("Duration")]
    public string? Duration { get; set; }

    [DisplayField("Provider Last Name")]
    public string? ProviderLastName { get; set; }

    [DisplayField("Provider First Name")]
    public string? ProviderFirstName { get; set; }

    [DisplayField("File Name")]
    public string? FileName { get; set; }

    [DisplayField("Row Number")]
    public int? RowNumber { get; set; }

    [DisplayField("Provider ID")]
    public int? ProviderId { get; set; }

    [DisplayField("Entry ID")]
    public int? EntryId { get; set; }

    [DisplayField("Billed Rate")]
    public decimal? BilledRate { get; set; }

    [DisplayField("Billed Amount")]
    public decimal? BilledAmount { get; set; }

    [DisplayField("Billed Date")]
    public DateTime? BilledDate { get; set; }

    [DisplayField("Billed Paid Date")]
    public DateTime? BilledPaidDate { get; set; }

    [DisplayField("Provider Rate")]
    public decimal? ProviderRate { get; set; }

    [DisplayField("Provider Amount")]
    public decimal? ProviderAmount { get; set; }

    [DisplayField("Provider Paid Date")]
    public DateTime? ProviderPaidDate { get; set; }

    [DisplayField("Is Overlap")]
    public bool IsOverlap { get; set; }

    [DisplayField("Voucher")]
    public string? Voucher { get; set; }

    [DisplayField("Voucher Amount")]
    public decimal? VoucherAmount { get; set; }

    [DisplayField("Voucher Balance Paid")]
    public DateTime? VoucherBalancePaid { get; set; }

    [DisplayField("Is Over Mandate")]
    public bool IsOverMandate { get; set; }

    [DisplayField("Is Over Duration")]
    public bool IsOverDuration { get; set; }

    [DisplayField("Is Under Group")]
    public bool IsUnderGroup { get; set; }
}
