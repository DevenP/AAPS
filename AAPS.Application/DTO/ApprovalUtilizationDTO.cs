using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public class ApprovalUtilizationDTO
{
    [DisplayField("Student ID")]
    public string? StudentId { get; set; }

    [DisplayField("Student Name")]
    public string? StudentName { get; set; }

    [DisplayField("Service Type")]
    public string? ServiceType { get; set; }

    [DisplayField("Provider")]
    public string? Provider { get; set; }

    [DisplayField("Frequency")]
    public string? Frequency { get; set; }

    [DisplayField("Expected Sessions")]
    public int ExpectedSessions { get; set; }

    [DisplayField("Delivered Sessions")]
    public int ActualSessions { get; set; }

    [DisplayField("Utilization %")]
    public int UtilizationPct { get; set; }

    [DisplayField("Under Utilized")]
    public bool IsUnderUtilized { get; set; }
}
