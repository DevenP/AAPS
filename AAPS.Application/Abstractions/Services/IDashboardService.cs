namespace AAPS.Application.Abstractions.Services;

public record DashboardStats
{
    public int TotalProviders { get; init; }
    public int VendorPortalDiscrepancies { get; init; }
    public int EvalsPendingPayment { get; init; }
    public int OperationAlerts { get; init; }
}

public record OperationAlertItem
{
    public string? StudentLastName { get; init; }
    public string? StudentFirstName { get; init; }
    public DateTime? ServiceDate { get; init; }
    public string? ProviderLastName { get; init; }
    public string? ProviderFirstName { get; init; }
    public string? Issue { get; init; }
}

public record DiscrepancyItem
{
    public string? StudentLastName { get; init; }
    public string? StudentFirstName { get; init; }
    public string? ProviderLastName { get; init; }
    public string? ProviderFirstName { get; init; }
    public string? AssignId { get; init; }
    public DateTime? StartDate { get; init; }
}

public record EvalPendingItem
{
    public string? StudentLastName { get; init; }
    public string? StudentFirstName { get; init; }
    public string? ServiceType { get; init; }
    public DateTime? BilledDate { get; init; }
}

public interface IDashboardService
{
    Task<DashboardStats> GetStatsAsync(CancellationToken ct = default);
    Task<List<OperationAlertItem>> GetOperationAlertsAsync(int limit = 15, CancellationToken ct = default);
    Task<List<DiscrepancyItem>> GetVendorPortalDiscrepanciesAsync(int limit = 15, CancellationToken ct = default);
    Task<List<EvalPendingItem>> GetEvalsPendingPaymentAsync(int limit = 15, CancellationToken ct = default);
}
