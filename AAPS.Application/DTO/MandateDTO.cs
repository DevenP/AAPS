using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record MandateDTO
{
    [DisplayField("Id")]
    public int Id { get; set; }

    [DisplayField("Conference Date")]
    public DateTime? ConferenceDate { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Last Name")]
    public string? LastName { get; set; }

    [DisplayField("First Name")]
    public string? FirstName { get; set; }

    [DisplayField("Home District")]
    public string? HomeDistrict { get; set; }

    [DisplayField("CSE")]
    public string? Cse { get; set; }

    [DisplayField("CSE District")]
    public string? CseDistrict { get; set; }

    [DisplayField("Grade")]
    public string? Grade { get; set; }

    [DisplayField("Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [DisplayField("Admin DBN")]
    public string? AdminDbn { get; set; }

    [DisplayField("D75")]
    public string? D75 { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Language")]
    public string? Language { get; set; }

    [DisplayField("Group Size")]
    public string? GroupSize { get; set; }

    [DisplayField("Duration")]
    public string? Duration { get; set; }

    [DisplayField("Service Location")]
    public string? ServiceLocation { get; set; }

    [DisplayField("Remaining Freq")]
    public string? RemainingFrequency { get; set; }

    [DisplayField("Provider")]
    public string? Provider { get; set; }

    [DisplayField("First Attend Date")]
    public DateTime? FirstAttendDate { get; set; }

    [DisplayField("Mandate ID")]
    public string? MandateId { get; set; }

    [DisplayField("Primary Phone 1")]
    public string? PrimaryPhone1 { get; set; }

    [DisplayField("Primary Phone 2")]
    public string? PrimaryPhone2 { get; set; }

    [DisplayField("Mandate Start")]
    public DateTime? MandateStart { get; set; }

    [DisplayField("Mandate End")]
    public DateTime? MandateEnd { get; set; }

    [DisplayField("File Name", browsable: false)]
    public string? FileName { get; set; }

    [DisplayField("Row Number", browsable: false)]
    public int? RowNumber { get; set; }

    [DisplayField("Service Start Date")]
    public DateTime? ServiceStartDate { get; set; }

    [DisplayField("Mismatched")]
    public bool IsMismatched { get; set; }
}
