namespace AAPS.Application.Abstractions.Services;

public record DashboardStats
{
    public int ActiveProviders { get; init; }
    public int OperationAlerts { get; init; }
    public int UnbilledSessions { get; init; }
    public decimal UnbilledAmount { get; init; }
    public int ExpiringApprovals { get; init; }
    public int VendorPortalDiscrepancies { get; init; }
    public int EvalsPendingPayment { get; init; }
    public int ExpiringLicenses { get; init; }
}

public record OperationAlertItem
{
    public string? StudentLastName { get; init; }
    public string? StudentFirstName { get; init; }
    public DateTime? ServiceDate { get; init; }
    public string? ProviderLastName { get; init; }
    public string? ProviderFirstName { get; init; }
    public bool MandateFlag { get; init; }
    public bool ProviderFlag { get; init; }
    public bool IsOverlap { get; init; }
    public bool IsOverMandate { get; init; }
    public bool IsOverDuration { get; init; }
    public bool IsUnderGroup { get; init; }
    public bool BRateFlag { get; init; }
    public bool PRateFlag { get; init; }
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

public record ExpiringApprovalItem
{
    public string? StudentId { get; init; }
    public string? StudentLastName { get; init; }
    public string? StudentFirstName { get; init; }
    public string? ServiceType { get; init; }
    public DateTime? MandateEnd { get; init; }
    public string? Provider { get; init; }
}

public record ExpiringLicenseItem
{
    public string? LastName { get; init; }
    public string? FirstName { get; init; }
    public string? LicenseNumber { get; init; }
    public DateTime ExpirationDate { get; init; }
}

public interface IDashboardService
{
    Task<DashboardStats> GetStatsAsync(CancellationToken ct = default);
    Task<List<OperationAlertItem>> GetOperationAlertsAsync(int limit = 15, CancellationToken ct = default);
    Task<List<DiscrepancyItem>> GetVendorPortalDiscrepanciesAsync(int limit = 15, CancellationToken ct = default);
    Task<List<EvalPendingItem>> GetEvalsPendingPaymentAsync(int limit = 15, CancellationToken ct = default);
    Task<List<ExpiringApprovalItem>> GetExpiringApprovalsAsync(int daysAhead = 30, int limit = 15, CancellationToken ct = default);
    Task<List<ExpiringLicenseItem>> GetExpiringLicensesAsync(int daysAhead = 60, int limit = 15, CancellationToken ct = default);
}
