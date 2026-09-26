using AAPS.Application.Common.Attributes;

namespace AAPS.Application.DTO;

public class ProviderPaySummaryDTO
{
    [DisplayField("Provider")]
    public string? Provider { get; set; }

    [DisplayField("Sessions")]
    public int Sessions { get; set; }

    [DisplayField("Total Provider Pay")]
    public decimal TotalProviderAmount { get; set; }

    [DisplayField("Unpaid Provider Pay")]
    public decimal UnpaidProviderAmount { get; set; }
}
