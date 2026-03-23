using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class BillingRateService : IBillingRateService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ILogger<BillingRateService> _logger;

    public BillingRateService(IDbContextFactory<AppDbContext> factory, ILogger<BillingRateService> logger)
    {
        _factory = factory;
        _logger = logger;
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

        _logger.LogInformation("Creating billing rate {District}/{ServiceType}/{Language} at {Rate:C2}",
            dto.District, dto.ServiceType, dto.Language, dto.Rate);

        // Guard: rate must be >= 1 (proc: IF @Rate<1 RETURN)
        if (dto.Rate < 1)
            throw new InvalidOperationException("Rate must be at least $1.00.");

        // Guard: duplicate combo (any record — active or historical — for this combo blocks insert)
        var exists = await db.BillingRates.AnyAsync(b =>
            b.District == dto.District &&
            b.ServiceType == dto.ServiceType &&
            b.Lang == dto.Language, ct);

        if (exists)
        {
            _logger.LogWarning("Billing rate already exists for {District}/{ServiceType}/{Language}",
                dto.District, dto.ServiceType, dto.Language);
            throw new InvalidOperationException(
                "A rate for this District / Service Type / Language combination already exists. " +
                "Use Edit to update the rate.");
        }

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

        _logger.LogInformation("Billing rate {Id} created for {District}/{ServiceType}/{Language} at {Rate:C2}",
            entity.BillingRate_Id, dto.District, dto.ServiceType, dto.Language, dto.Rate);

        // Cascade bRate + bAmount to matching unpaid Sesis rows (proc: bPaid IS NULL)
        // Duration and Actual_Size are varchar — must use raw SQL for the CONVERT
        var sesisCount = await db.Database.ExecuteSqlRawAsync(
            @"UPDATE Sesis
              SET bRate   = @rate,
                  bAmount = @rate * CONVERT(int, Duration) / 60.0 / CONVERT(int, Actual_Size)
              WHERE Service_Type       = @serviceType
                AND GDistrict          = @district
                AND Language_Provided  = @lang
                AND bPaid IS NULL",
            new SqlParameter("@rate",        dto.Rate        ?? 0m),
            new SqlParameter("@serviceType", dto.ServiceType ?? ""),
            new SqlParameter("@district",    dto.District    ?? ""),
            new SqlParameter("@lang",        dto.Language    ?? ""));

        _logger.LogInformation("Cascaded rate to {Count} Sesis records", sesisCount);

        // Cascade bAmount to matching unpaid Evals rows (proc: bPaid IS NULL)
        var evalsCount = await db.Database.ExecuteSqlRawAsync(
            @"UPDATE Evals
              SET bAmount = @rate
              WHERE RTRIM(ServiceType) = RTRIM(@serviceType)
                AND RTRIM(District)   = RTRIM(@district)
                AND RTRIM(Language)   = RTRIM(@lang)
                AND bPaid IS NULL",
            new SqlParameter("@rate",        dto.Rate        ?? 0m),
            new SqlParameter("@serviceType", dto.ServiceType ?? ""),
            new SqlParameter("@district",    dto.District    ?? ""),
            new SqlParameter("@lang",        dto.Language    ?? ""));

        _logger.LogInformation("Cascaded rate to {Count} Evals records", evalsCount);

        return entity.BillingRate_Id;
    }

    public async Task UpdateAsync(int id, BillingRateDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var existing = await db.BillingRates.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException("Billing rate not found.");

        _logger.LogInformation("Updating billing rate {Id}: rate change from {OldRate:C2} to {NewRate:C2} for {District}/{ServiceType}/{Language}",
            id, existing.Rate, dto.Rate, existing.District, existing.ServiceType, existing.Lang);

        // Guard: rate unchanged
        if (existing.Rate == dto.Rate)
        {
            _logger.LogWarning("Billing rate {Id} update skipped — rate unchanged at {Rate:C2}", id, existing.Rate);
            throw new InvalidOperationException("The new rate is the same as the current rate. No changes were made.");
        }

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

        _logger.LogInformation("Billing rate {Id} deactivated, new rate record {NewId} created", id, newEntity.BillingRate_Id);
        // Note: the original stored proc does not cascade rate changes to Sesis/Evals on update
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.BillingRates.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _logger.LogInformation("Deleting billing rate {Id} ({District}/{ServiceType}/{Language} at {Rate:C2})",
                id, entity.District, entity.ServiceType, entity.Lang, entity.Rate);
            db.BillingRates.Remove(entity);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Billing rate {Id} deleted", id);
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
