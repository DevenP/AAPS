using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record SesiDTO
{
    [DisplayField("Id", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Student Name")]
    public string? StudentLastName { get; set; }

    [DisplayField("Student First Name", browsable: false)]
    public string? StudentFirstName { get; set; }

    [DisplayField("Grade", browsable: false)]
    public string? Grade { get; set; }

    [DisplayField("Date of Birth", browsable: false)]
    public DateTime? DateOfBirth { get; set; }

    [DisplayField("Home District", browsable: false)]
    public string? HomeDistrict { get; set; }

    [DisplayField("CSE", browsable: false)]
    public string? Cse { get; set; }

    [DisplayField("CSE District", browsable: false)]
    public string? CseDistrict { get; set; }

    [DisplayField("Admin DBN", browsable: false)]
    public string? AdminDbn { get; set; }

    [DisplayField("G District")]
    public string? GDistrict { get; set; }

    [DisplayField("Borough", browsable: false)]
    public string? Borough { get; set; }

    [DisplayField("Mandate Short", browsable: false)]
    public string? MandateShort { get; set; }

    [DisplayField("Mandate Start", browsable: false)]
    public DateTime? MandateStart { get; set; }

    [DisplayField("Mandate End", browsable: false)]
    public DateTime? MandateEnd { get; set; }

    [DisplayField("Mandated Max Group", browsable: false)]
    public string? MandatedMaxGroup { get; set; }

    [DisplayField("Assignment First Encounter", browsable: false)]
    public DateTime? AssignmentFirstEncounter { get; set; }

    [DisplayField("Frequency")]
    public string? AssignmentClaimed { get; set; }

    [DisplayField("Service Date")]
    public DateTime? DateOfService { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Language")]
    public string? LanguageProvided { get; set; }

    [DisplayField("Session Type", browsable: false)]
    public string? SessionType { get; set; }

    [DisplayField("Session Notes", browsable: false)]
    public string? SessionNotes { get; set; }

    [DisplayField("Grouping", browsable: false)]
    public string? Grouping { get; set; }

    [DisplayField("Group Size")]
    public string? ActualSize { get; set; }

    [DisplayField("Start Time")]
    public string? StartTime { get; set; }

    [DisplayField("End Time")]
    public string? EndTime { get; set; }

    [DisplayField("Duration")]
    public string? Duration { get; set; }

    [DisplayField("Provider")]
    public string? ProviderLastName { get; set; }

    [DisplayField("Provider First Name", browsable: false)]
    public string? ProviderFirstName { get; set; }

    [DisplayField("File Name", browsable: false)]
    public string? FileName { get; set; }

    [DisplayField("Row Number", browsable: false)]
    public int? RowNumber { get; set; }

    [DisplayField("Provider ID", browsable: false)]
    public int? ProviderId { get; set; }

    [DisplayField("Approval ID")]
    public int? EntryId { get; set; }

    [DisplayField("Billed Rate", browsable: false)]
    public decimal? BilledRate { get; set; }

    [DisplayField("Billed Amount", browsable: false)]
    public decimal? BilledAmount { get; set; }

    [DisplayField("Billed Date")]
    public DateTime? BilledDate { get; set; }

    [DisplayField("Billed Paid Date", browsable: false)]
    public DateTime? BilledPaidDate { get; set; }

    [DisplayField("Provider Rate", browsable: false)]
    public decimal? ProviderRate { get; set; }

    [DisplayField("Provider Amount", browsable: false)]
    public decimal? ProviderAmount { get; set; }

    [DisplayField("Provider Paid Date")]
    public DateTime? ProviderPaidDate { get; set; }

    [DisplayField("Is Overlap", browsable: false)]
    public bool IsOverlap { get; set; }

    [DisplayField("Voucher")]
    public string? Voucher { get; set; }

    [DisplayField("Voucher Amount", browsable: false)]
    public decimal? VoucherAmount { get; set; }

    [DisplayField("Voucher Balance Paid", browsable: false)]
    public DateTime? VoucherBalancePaid { get; set; }

    [DisplayField("Is Over Mandate", browsable: false)]
    public bool IsOverMandate { get; set; }

    [DisplayField("Is Over Duration", browsable: false)]
    public bool IsOverDuration { get; set; }

    [DisplayField("Is Under Group", browsable: false)]
    public bool IsUnderGroup { get; set; }
}
