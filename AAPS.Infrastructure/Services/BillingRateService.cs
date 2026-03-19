using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class BillingRateService : IBillingRateService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public BillingRateService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<PagedResult<BillingRateDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var baseQuery = db.BillingRates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            baseQuery = baseQuery.Where(b =>
                (b.District != null && b.District.Contains(term)) ||
                (b.ServiceType != null && b.ServiceType.Contains(term)) ||
                (b.Lang != null && b.Lang.Contains(term)));
        }

        var query = baseQuery.Select(ToDTO);
        return await query.ToPagedResultAsync(request, ct, performSearch: false);
    }

    public async Task<BillingRateDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.BillingRates
            .AsNoTracking()
            .Where(b => b.BillingRate_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(BillingRateDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        // Guard: duplicate combo (any record — active or historical — for this combo blocks insert)
        var exists = await db.BillingRates.AnyAsync(b =>
            b.District == dto.District &&
            b.ServiceType == dto.ServiceType &&
            b.Lang == dto.Language, ct);

        if (exists)
            throw new InvalidOperationException(
                "A rate for this District / Service Type / Language combination already exists. " +
                "Use Edit to update the rate.");

        // Insert new active rate
        var entity = new BillingRate
        {
            District    = dto.District,
            ServiceType = dto.ServiceType,
            Lang        = dto.Language,
            Rate        = dto.Rate,
            Effective   = DateTime.Now,
            Active      = true
        };
        db.BillingRates.Add(entity);
        await db.SaveChangesAsync(ct);

        // Cascade bRate + bAmount to matching Sesis rows
        // Duration and Actual_Size are varchar — must use raw SQL for the CONVERT
        await db.Database.ExecuteSqlRawAsync(
            @"UPDATE Sesis
              SET bRate   = @rate,
                  bAmount = @rate * CONVERT(int, Duration) / 60.0 / CONVERT(int, Actual_Size)
              WHERE Service_Type       = @serviceType
                AND GDistrict          = @district
                AND Language_Provided  = @lang",
            new SqlParameter("@rate",        dto.Rate        ?? 0m),
            new SqlParameter("@serviceType", dto.ServiceType ?? ""),
            new SqlParameter("@district",    dto.District    ?? ""),
            new SqlParameter("@lang",        dto.Language    ?? ""));

        // Cascade bAmount to matching Evals rows (RTRIM matches proc behaviour)
        await db.Database.ExecuteSqlRawAsync(
            @"UPDATE Evals
              SET bAmount = @rate
              WHERE RTRIM(ServiceType) = RTRIM(@serviceType)
                AND RTRIM(District)   = RTRIM(@district)
                AND RTRIM(Language)   = RTRIM(@lang)",
            new SqlParameter("@rate",        dto.Rate        ?? 0m),
            new SqlParameter("@serviceType", dto.ServiceType ?? ""),
            new SqlParameter("@district",    dto.District    ?? ""),
            new SqlParameter("@lang",        dto.Language    ?? ""));

        return entity.BillingRate_Id;
    }

    public async Task UpdateAsync(int id, BillingRateDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var existing = await db.BillingRates.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException("Billing rate not found.");

        // Guard: rate unchanged
        if (existing.Rate == dto.Rate)
            throw new InvalidOperationException("The new rate is the same as the current rate. No changes were made.");

        // Deactivate the old row (audit trail — matches proc behaviour)
        existing.Active = null;

        // Insert new row with same combo but updated rate
        var newEntity = new BillingRate
        {
            District    = existing.District,
            ServiceType = existing.ServiceType,
            Lang        = existing.Lang,
            Rate        = dto.Rate,
            Effective   = DateTime.Now,
            Active      = true
        };
        db.BillingRates.Add(newEntity);
        await db.SaveChangesAsync(ct);
        // Note: the original stored proc does not cascade rate changes to Sesis/Evals on update
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.BillingRates.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.BillingRates.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<BillingRateUsage> GetUsageCountAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var rate = await db.BillingRates
            .AsNoTracking()
            .Where(b => b.BillingRate_Id == id)
            .FirstOrDefaultAsync(ct);

        if (rate == null) return new BillingRateUsage(0, 0);

        var sesiCount = await db.Seses.CountAsync(s =>
            s.Service_Type      == rate.ServiceType &&
            s.GDistrict         == rate.District &&
            s.Language_Provided == rate.Lang, ct);

        var evalCount = await db.Evals.CountAsync(e =>
            e.ServiceType == rate.ServiceType &&
            e.District    == rate.District &&
            e.Language    == rate.Lang, ct);

        return new BillingRateUsage(sesiCount, evalCount);
    }

    private static readonly Expression<Func<BillingRate, BillingRateDTO>> ToDTO = b => new BillingRateDTO
    {
        Id            = b.BillingRate_Id,
        District      = b.District,
        ServiceType   = b.ServiceType,
        Rate          = b.Rate,
        EffectiveDate = b.Effective,
        IsActive      = b.Active ?? false,
        Language      = b.Lang
    };
}
