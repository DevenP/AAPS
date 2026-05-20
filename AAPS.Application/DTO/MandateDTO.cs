using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public record MandateDTO
{
    [DisplayField("Approval ID", browsable: false, IsReadOnly = true)]
    public int Id { get; set; }

    [DisplayField("Conf Date", GroupName = "Dates")]
    public DateTime? ConferenceDate { get; set; }

    [DisplayField("Student ID", GroupName = "Student")]
    public string? StudentId { get; set; }

    [DisplayField("Student Name", GroupName = "Student")]
    public string? LastName { get; set; }

    [DisplayField("First Name", browsable: false)]
    public string? FirstName { get; set; }

    [DisplayField("Home District", GroupName = "Student")]
    public string? HomeDistrict { get; set; }

    [DisplayField("CSE", GroupName = "Student")]
    public string? Cse { get; set; }

    [DisplayField("CSE District", GroupName = "Student")]
    public string? CseDistrict { get; set; }

    [DisplayField("Grade", GroupName = "Student")]
    public string? Grade { get; set; }

    [DisplayField("Date of Birth", GroupName = "Student")]
    public DateTime? DateOfBirth { get; set; }

    [DisplayField("Admin DBN", GroupName = "Student")]
    public string? AdminDbn { get; set; }

    [DisplayField("D75", GroupName = "Student")]
    public string? D75 { get; set; }

    [DisplayField("Service Type", GroupName = "Service")]
    public string? ServiceType { get; set; }

    [DisplayField("Lang", GroupName = "Service")]
    public string? Language { get; set; }

    [DisplayField("Grp Size", GroupName = "Service")]
    public string? GroupSize { get; set; }

    [DisplayField("Dur", GroupName = "Service")]
    public string? Duration { get; set; }

    [DisplayField("Service Location", GroupName = "Service")]
    public string? ServiceLocation { get; set; }

    [DisplayField("Remaining Frequency", GroupName = "Service")]
    public string? RemainingFrequency { get; set; }

    [DisplayField("Provider", GroupName = "Provider")]
    public string? Provider { get; set; }

    [DisplayField("First Attend Date", GroupName = "Provider")]
    public DateTime? FirstAttendDate { get; set; }

    [DisplayField("Mandate ID", GroupName = "Provider")]
    public string? MandateId { get; set; }

    [DisplayField("Assignment ID", GroupName = "Provider")]
    public string? AssignmentId { get; set; }

    [DisplayField("Primary Contact Phone 1", GroupName = "Parent")]
    public string? PrimaryPhone1 { get; set; }

    [DisplayField("Primary Contact Phone 2", GroupName = "Parent")]
    public string? PrimaryPhone2 { get; set; }

    [DisplayField("IEP Type", GroupName = "School")]
    public string? IepType { get; set; }

    [DisplayField("School Name", GroupName = "School")]
    public string? SchoolName { get; set; }

    [DisplayField("Agency Name", GroupName = "School")]
    public string? AgencyName { get; set; }

    [DisplayField("Auth Physical DBN", GroupName = "School")]
    public string? AuthPhysicalDbn { get; set; }

    [DisplayField("Parent Name", GroupName = "Parent")]
    public string? ParentLastName { get; set; }

    [DisplayField("Parent First Name", browsable: false)]
    public string? ParentFirstName { get; set; }

    [DisplayField("Parent Email", GroupName = "Parent")]
    public string? ParentEmail { get; set; }

    [DisplayField("Mandate Start", GroupName = "Dates")]
    public DateTime? MandateStart { get; set; }

    [DisplayField("Mandate End", GroupName = "Dates")]
    public DateTime? MandateEnd { get; set; }

    [DisplayField("Service Start Date", GroupName = "Dates")]
    public DateTime? ServiceStartDate { get; set; }

    [DisplayField("File Name", GroupName = "School")]
    public string? FileName { get; set; }

    [DisplayField("Row Number", GroupName = "School")]
    public int? RowNumber { get; set; }

    [DisplayField("Mismatched", GroupName = "Student")]
    public bool IsMismatched { get; set; }
}
