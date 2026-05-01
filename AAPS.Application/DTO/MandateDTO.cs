using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record MandateDTO
{
    [DisplayField("Approval ID", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("Conf Date")]
    public DateTime? ConferenceDate { get; set; }

    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Student Name")]
    public string? LastName { get; set; }

    [DisplayField("First Name", browsable: false)]
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

    [DisplayField("Lang")]
    public string? Language { get; set; }

    [DisplayField("Grp Size")]
    public string? GroupSize { get; set; }

    [DisplayField("Dur")]
    public string? Duration { get; set; }

    [DisplayField("Service Location")]
    public string? ServiceLocation { get; set; }

    [DisplayField("Remaining Frequency")]
    public string? RemainingFrequency { get; set; }

    [DisplayField("Provider")]
    public string? Provider { get; set; }

    [DisplayField("First Attend Date")]
    public DateTime? FirstAttendDate { get; set; }

    [DisplayField("Mandate ID")]
    public string? MandateId { get; set; }

    [DisplayField("Primary Contact Phone 1")]
    public string? PrimaryPhone1 { get; set; }

    [DisplayField("Primary Contact Phone 2")]
    public string? PrimaryPhone2 { get; set; }

    [DisplayField("IEP Type")]
    public string? IepType { get; set; }

    [DisplayField("School Name")]
    public string? SchoolName { get; set; }

    [DisplayField("Agency Name")]
    public string? AgencyName { get; set; }

    [DisplayField("Auth Physical DBN")]
    public string? AuthPhysicalDbn { get; set; }

    [DisplayField("Assignment ID")]
    public string? AssignmentId { get; set; }

    [DisplayField("Parent Name")]
    public string? ParentLastName { get; set; }

    [DisplayField("Parent First Name", browsable: false)]
    public string? ParentFirstName { get; set; }

    [DisplayField("Parent Email")]
    public string? ParentEmail { get; set; }

    [DisplayField("Mandate Start")]
    public DateTime? MandateStart { get; set; }

    [DisplayField("Mandate End")]
    public DateTime? MandateEnd { get; set; }

    [DisplayField("File Name")]
    public string? FileName { get; set; }

    [DisplayField("Row Number")]
    public int? RowNumber { get; set; }

    [DisplayField("Service Start Date")]
    public DateTime? ServiceStartDate { get; set; }

    [DisplayField("Mismatched")]
    public bool IsMismatched { get; set; }
}
