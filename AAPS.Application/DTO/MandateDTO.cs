using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.DTO;

public record MandateDTO
{
    [Browsable(true)]
    [Display(Name = "Id")]
    public int Id { get; set; }

    [Browsable(true)]
    [Display(Name = "ConferenceDate")]
    public DateTime? ConferenceDate { get; set; }

    [Browsable(true)]
    [Display(Name = "StudentId")]
    public string? StudentId { get; set; }

    [Browsable(true)]
    [Display(Name = "LastName")]
    public string? LastName { get; set; }

    [Browsable(true)]
    [Display(Name = "FirstName")]
    public string? FirstName { get; set; }

    [Browsable(true)]
    [Display(Name = "HomeDistrict")]
    public string? HomeDistrict { get; set; }

    [Browsable(true)]
    [Display(Name = "Cse")]
    public string? Cse { get; set; }

    [Browsable(true)]
    [Display(Name = "CseDistrict")]
    public string? CseDistrict { get; set; }

    [Browsable(true)]
    [Display(Name = "Grade")]
    public string? Grade { get; set; }

    [Browsable(true)]
    [Display(Name = "DateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [Browsable(true)]
    [Display(Name = "AdminDbn")]
    public string? AdminDbn { get; set; }

    [Browsable(true)]
    [Display(Name = "D75")]
    public string? D75 { get; set; }

    [Browsable(true)]
    [Display(Name = "ServiceType")]
    public string? ServiceType { get; set; }

    [Browsable(true)]
    [Display(Name = "Language")]
    public string? Language { get; set; }

    [Browsable(true)]
    [Display(Name = "GroupSize")]
    public string? GroupSize { get; set; }

    [Browsable(true)]
    [Display(Name = "Duration")]
    public string? Duration { get; set; }

    [Browsable(true)]
    [Display(Name = "ServiceLocation")]
    public string? ServiceLocation { get; set; }

    [Browsable(true)]
    [Display(Name = "RemainingFrequency")]
    public string? RemainingFrequency { get; set; }

    [Browsable(true)]
    [Display(Name = "Provider")]
    public string? Provider { get; set; }

    [Browsable(true)]
    [Display(Name = "FirstAttendDate")]
    public DateTime? FirstAttendDate { get; set; }

    [Browsable(true)]
    [Display(Name = "MandateId")]
    public string? MandateId { get; set; }

    [Browsable(true)]
    [Display(Name = "PrimaryPhone1")]
    public string? PrimaryPhone1 { get; set; }

    [Browsable(true)]
    [Display(Name = "PrimaryPhone2")]
    public string? PrimaryPhone2 { get; set; }

    [Browsable(true)]
    [Display(Name = "MandateStart")]
    public DateTime? MandateStart { get; set; }

    [Browsable(true)]
    [Display(Name = "MandateEnd")]
    public DateTime? MandateEnd { get; set; }

    [Browsable(true)]
    [Display(Name = "FileName")]
    public string? FileName { get; set; }

    [Browsable(true)]
    [Display(Name = "RowNumber")]
    public int? RowNumber { get; set; }

    [Browsable(true)]
    [Display(Name = "ServiceStartDate")]
    public DateTime? ServiceStartDate { get; set; }

    //

    [Browsable(true)]
    [Display(Name = "IsMismatched")]
    public bool IsMismatched { get; set; }
}
