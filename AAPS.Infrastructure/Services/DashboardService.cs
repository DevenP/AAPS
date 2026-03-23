using AAPS.Application.Abstractions.Services;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AAPS.Infrastructure.Services;

// Minimal projection for the VendorPortal_Select stored proc columns needed by the dashboard
internal sealed class VendorPortalDiscrepancyRaw
{
    public string? Last_Name { get; set; }
    public string? First_Name { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? Assign_Id { get; set; }
    public DateTime? pStartDate { get; set; }
    public int? Entry_Id { get; set; }
    // Required by SqlQueryRaw — proc always returns these columns
    public int VendorPortal_Id { get; set; }
    public string? pSsn { get; set; }
    public string? pBoro { get; set; }
    public string? pDist { get; set; }
    public string? pSchool { get; set; }
    public string? pFund { get; set; }
    public string? Student_ID { get; set; }
    public string? pDur { get; set; }
    public string? pFreq { get; set; }
    public string? pGrpSize { get; set; }
    public string? VPFile { get; set; }
    public string? Mismatch { get; set; }
}

public class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IDbContextFactory<AppDbContext> factory, ILogger<DashboardService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<DashboardStats> GetStatsAsync(CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var today = DateTime.Today;

        var totalProviders = await db.Providers.CountAsync(p => p.Status == "Active", ct);

        var discrepancies = await db.VendorPortals
            .CountAsync(v => v.Entry_Id == null, ct);

        var evalsPending = await db.Evals
            .CountAsync(e => e.Billed != null && e.bPaid == null, ct);

        // All 8 alert flag conditions — matches Operations page flags
        var operationAlerts = await db.Seses.CountAsync(s =>
            s.Entry_Id == null ||
            s.Provider_Id == null ||
            s.Overlap == true ||
            s.OverMandate == true ||
            s.OverDuration == true ||
            s.UnderGroup == true ||
            s.bRate == null ||
            s.pRate == null, ct);

        // Sessions with a billing rate set but not yet billed and not yet paid
        var unbilledSessions = await db.Seses.CountAsync(s =>
            s.bRate != null && s.Billed == null && s.bPaid == null, ct);

        var unbilledAmount = await db.Seses
            .Where(s => s.bRate != null && s.Billed == null && s.bPaid == null)
            .SumAsync(s => (decimal?)s.bAmount, ct) ?? 0m;

        // Approvals expiring within the next 30 days
        var cutoff = today.AddDays(30);
        var expiringApprovals = await db.Mandates.CountAsync(m =>
            m.MandateEnd != null && m.MandateEnd >= today && m.MandateEnd <= cutoff, ct);

        // Providers with a license expiring within 60 days (or already expired)
        var licenseCutoff = today.AddDays(60);
        var expiringLicenses = await db.Providers.CountAsync(p =>
            (p.License1Exp != null && p.License1Exp <= licenseCutoff) ||
            (p.License2Exp != null && p.License2Exp <= licenseCutoff), ct);

        var stats = new DashboardStats
        {
            ActiveProviders           = totalProviders,
            VendorPortalDiscrepancies = discrepancies,
            EvalsPendingPayment       = evalsPending,
            OperationAlerts           = operationAlerts,
            UnbilledSessions          = unbilledSessions,
            UnbilledAmount            = unbilledAmount,
            ExpiringApprovals         = expiringApprovals,
            ExpiringLicenses          = expiringLicenses,
        };

        _logger.LogInformation(
            "Dashboard stats: {Alerts} alerts, {Unbilled} unbilled, {Expiring} expiring approvals, {Licenses} expiring licenses",
            operationAlerts, unbilledSessions, expiringApprovals, expiringLicenses);

        return stats;
    }

    public async Task<List<OperationAlertItem>> GetOperationAlertsAsync(int limit = 15, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        return await db.Seses
            .AsNoTracking()
            .Where(s =>
                s.Entry_Id == null ||
                s.Provider_Id == null ||
                s.Overlap == true ||
                s.OverMandate == true ||
                s.OverDuration == true ||
                s.UnderGroup == true ||
                s.bRate == null ||
                s.pRate == null)
            .OrderByDescending(s => s.date_of_Service)
            .Take(limit)
            .Select(s => new OperationAlertItem
            {
                StudentLastName  = s.Last_Name,
                StudentFirstName = s.First_Name,
                ServiceDate      = s.date_of_Service,
                ProviderLastName  = s.Provider_Last_Name,
                ProviderFirstName = s.Provider_First_Name,
                MandateFlag  = s.Entry_Id == null,
                ProviderFlag = s.Provider_Id == null,
                IsOverlap    = s.Overlap == true,
                IsOverMandate  = s.OverMandate == true,
                IsOverDuration = s.OverDuration == true,
                IsUnderGroup   = s.UnderGroup == true,
                BRateFlag = s.bRate == null,
                PRateFlag = s.pRate == null,
            })
            .ToListAsync(ct);
    }

    public async Task<List<DiscrepancyItem>> GetVendorPortalDiscrepanciesAsync(int limit = 15, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        // @unbound=1 returns only rows where Entry_Id IS NULL, with student/provider names from the JOIN
        var raw = await db.Database
            .SqlQueryRaw<VendorPortalDiscrepancyRaw>(
                "EXEC [dbo].[VendorPortal_Select] @searchBy=0, @searchByValue=NULL, @dateSearch=0, @from=NULL, @to=NULL, @unbound=1")
            .ToListAsync(ct);

        return raw
            .Take(limit)
            .Select(r => new DiscrepancyItem
            {
                StudentLastName   = r.Last_Name,
                StudentFirstName  = r.First_Name,
                ProviderLastName  = r.LastName,
                ProviderFirstName = r.FirstName,
                AssignId  = r.Assign_Id,
                StartDate = r.pStartDate
            })
            .ToList();
    }

    public async Task<List<EvalPendingItem>> GetEvalsPendingPaymentAsync(int limit = 15, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        return await db.Evals
            .AsNoTracking()
            .Where(e => e.Billed != null && e.bPaid == null)
            .OrderBy(e => e.Billed)
            .Take(limit)
            .Select(e => new EvalPendingItem
            {
                StudentLastName  = e.StudentLast,
                StudentFirstName = e.StudentFirst,
                ServiceType = e.ServiceType,
                BilledDate  = e.Billed
            })
            .ToListAsync(ct);
    }

    public async Task<List<ExpiringApprovalItem>> GetExpiringApprovalsAsync(int daysAhead = 30, int limit = 15, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var today  = DateTime.Today;
        var cutoff = today.AddDays(daysAhead);

        return await db.Mandates
            .AsNoTracking()
            .Where(m => m.MandateEnd != null && m.MandateEnd >= today && m.MandateEnd <= cutoff)
            .OrderBy(m => m.MandateEnd)
            .Take(limit)
            .Select(m => new ExpiringApprovalItem
            {
                StudentId        = m.Student_ID,
                StudentLastName  = m.Last_Name,
                StudentFirstName = m.First_Name,
                ServiceType = m.Service_Type,
                MandateEnd  = m.MandateEnd,
                Provider    = m.Provider
            })
            .ToListAsync(ct);
    }

    public async Task<List<ExpiringLicenseItem>> GetExpiringLicensesAsync(int daysAhead = 60, int limit = 15, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var cutoff = DateTime.Today.AddDays(daysAhead);

        // Flatten License1 and License2 into one list, take the soonest per provider
        var license1 = await db.Providers
            .AsNoTracking()
            .Where(p => p.License1Exp != null && p.License1Exp <= cutoff)
            .Select(p => new ExpiringLicenseItem
            {
                LastName       = p.LastName,
                FirstName      = p.FirstName,
                LicenseNumber  = p.License1,
                ExpirationDate = p.License1Exp!.Value
            })
            .ToListAsync(ct);

        var license2 = await db.Providers
            .AsNoTracking()
            .Where(p => p.License2Exp != null && p.License2Exp <= cutoff)
            .Select(p => new ExpiringLicenseItem
            {
                LastName       = p.LastName,
                FirstName      = p.FirstName,
                LicenseNumber  = p.License2,
                ExpirationDate = p.License2Exp!.Value
            })
            .ToListAsync(ct);

        return license1
            .Concat(license2)
            .OrderBy(x => x.ExpirationDate)
            .Take(limit)
            .ToList();
    }
}
