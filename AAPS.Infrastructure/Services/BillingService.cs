using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AAPS.Infrastructure.Services;

public class BillingService : IBillingService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ILogger<BillingService> _logger;

    public BillingService(IDbContextFactory<AppDbContext> factory, ILogger<BillingService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<PagedResult<BillingRecordDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var query = db.Seses.AsNoTracking()
            .Where(s => s.Overlap != true
                     && s.OverMandate != true
                     && s.OverDuration != true
                     && s.UnderGroup != true
                     && s.Entry_Id != null
                     && s.bRate != null
                     && s.pRate != null)
            .Join(db.Providers,
                s => s.Provider_Id,
                p => p.Provider_Id,
                (s, p) => new { s, p })
            .Where(sp => db.VendorPortals.Any(v =>
                v.Entry_Id == sp.s.Entry_Id &&
                v.pSsn == sp.p.Ssn!.Substring(0, 3) + sp.p.Ssn.Substring(4, 2) + sp.p.Ssn.Substring(7, 4) &&
                v.Assign_Id != null))
            .Select(sp => new BillingRecordDTO
            {
                SesisId        = sp.s.Sesis_Id,
                DateOfService  = sp.s.date_of_Service,
                StartTime      = sp.s.Start_Time,
                EndTime        = sp.s.End_Time,
                Provider       = sp.s.Provider_Last_Name + ", " + sp.s.Provider_First_Name,
                GDistrict      = sp.s.GDistrict,
                StudentId      = sp.s.Student_ID,
                Student        = sp.s.Last_Name + ", " + sp.s.First_Name,
                Grade          = sp.s.Grade,
                ServiceType    = sp.s.Service_Type,
                ActualSize     = sp.s.Actual_Size,
                Duration       = sp.s.Duration,
                Frequency      = sp.s.Assignment_Claimed,
                Billed         = sp.s.Billed,
                BilledPaidOn   = sp.s.bPaid,
                ProviderPaidOn = sp.s.pPaid,
                BillingRate    = sp.s.bRate,
                ProviderRate   = sp.s.pRate,
                BillingAmount  = sp.s.bAmount,
                ProviderAmount = sp.s.pAmount,
                AssignId = db.VendorPortals
                    .Where(v => v.Entry_Id == sp.s.Entry_Id &&
                                v.pSsn == sp.p.Ssn!.Substring(0, 3) + sp.p.Ssn.Substring(4, 2) + sp.p.Ssn.Substring(7, 4) &&
                                v.Assign_Id != null)
                    .OrderBy(v => v.VendorPortal_Id)
                    .Select(v => v.Assign_Id)
                    .FirstOrDefault(),
                EntryId = sp.s.Entry_Id,
            });

        return await query.ToPagedResultAsync(request, ct, performSearch: false);
    }

    public async Task UpdateBillingDatesAsync(int sesisId, DateTime? billed, DateTime? billedPaidOn, DateTime? providerPaidOn, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var entity = await db.Seses.FindAsync(new object[] { sesisId }, ct)
            ?? throw new KeyNotFoundException($"Sesi record {sesisId} not found.");

        _logger.LogInformation(
            "Updating billing dates for Sesis {SesisId}: Billed={Billed}, BilledPaid={BilledPaid}, ProviderPaid={ProviderPaid}",
            sesisId, billed, billedPaidOn, providerPaidOn);

        entity.Billed = billed;
        entity.bPaid  = billedPaidOn;
        entity.pPaid  = providerPaidOn;

        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Billing dates updated for Sesis {SesisId}", sesisId);
    }
}
