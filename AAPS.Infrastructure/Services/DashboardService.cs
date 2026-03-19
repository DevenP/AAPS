using AAPS.Application.Abstractions.Services;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;

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

    public DashboardService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<DashboardStats> GetStatsAsync(CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var totalProviders = await db.Providers.CountAsync(ct);

        var discrepancies = await db.VendorPortals
            .CountAsync(v => v.Entry_Id == null, ct);

        var evalsPending = await db.Evals
            .CountAsync(e => e.Billed != null && e.bPaid == null, ct);

        var operationAlerts = await db.Seses
            .CountAsync(s => s.Entry_Id == null || s.Provider_Id == null || s.bRate == null, ct);

        return new DashboardStats
        {
            TotalProviders = totalProviders,
            VendorPortalDiscrepancies = discrepancies,
            EvalsPendingPayment = evalsPending,
            OperationAlerts = operationAlerts
        };
    }

    public async Task<List<OperationAlertItem>> GetOperationAlertsAsync(int limit = 15, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        return await db.Seses
            .AsNoTracking()
            .Where(s => s.Entry_Id == null || s.Provider_Id == null || s.bRate == null)
            .OrderByDescending(s => s.date_of_Service)
            .Take(limit)
            .Select(s => new OperationAlertItem
            {
                StudentLastName = s.Last_Name,
                StudentFirstName = s.First_Name,
                ServiceDate = s.date_of_Service,
                ProviderLastName = s.Provider_Last_Name,
                ProviderFirstName = s.Provider_First_Name,
                Issue = s.Entry_Id == null && s.Provider_Id == null && s.bRate == null ? "No Entry / No Provider / No Rate"
                      : s.Entry_Id == null && s.Provider_Id == null ? "No Entry / No Provider"
                      : s.Entry_Id == null && s.bRate == null ? "No Entry / No Rate"
                      : s.Provider_Id == null && s.bRate == null ? "No Provider / No Rate"
                      : s.Entry_Id == null ? "No Entry"
                      : s.Provider_Id == null ? "No Provider"
                      : "No Rate"
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
                StudentLastName  = r.Last_Name,
                StudentFirstName = r.First_Name,
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
                StudentLastName = e.StudentLast,
                StudentFirstName = e.StudentFirst,
                ServiceType = e.ServiceType,
                BilledDate = e.Billed
            })
            .ToListAsync(ct);
    }
}
