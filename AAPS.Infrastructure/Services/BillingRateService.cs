using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class BillingRateService : IBillingRateService
{
    private readonly IAppDbContext _db;

    public BillingRateService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Application.Common.Paging.PagedResult<BillingRateDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        // Apply global search on the raw entity before projection so EF can
        // translate it against real indexed columns instead of DTO properties.
        var baseQuery = _db.BillingRates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            baseQuery = baseQuery.Where(b =>
                (b.District != null && b.District.Contains(term)) ||
                (b.ServiceType != null && b.ServiceType.Contains(term)) ||
                (b.Lang != null && b.Lang.Contains(term)));
        }

        var query = baseQuery.Select(ToDTO);

        // performSearch: false — search was already applied above on the raw entity
        return await query.ToPagedResultAsync(request, ct, performSearch: false);
    }


    public async Task<BillingRateDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.BillingRates
            .AsNoTracking()
            .Where(b => b.BillingRate_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(BillingRateDTO dto, CancellationToken ct = default)
    {
        var entity = new BillingRate
        {
            District = dto.District,
            ServiceType = dto.ServiceType,
            Rate = dto.Rate,
            Effective = dto.EffectiveDate,
            Active = dto.IsActive,
            Lang = dto.Language
        };
        _db.BillingRates.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.BillingRate_Id;
    }

    public async Task UpdateAsync(int id, BillingRateDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.BillingRates.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException();

        entity.District = dto.District;
        entity.ServiceType = dto.ServiceType;
        entity.Rate = dto.Rate;
        entity.Effective = dto.EffectiveDate;
        entity.Active = dto.IsActive;
        entity.Lang = dto.Language;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.BillingRates.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.BillingRates.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<BillingRate, BillingRateDTO>> ToDTO = b => new BillingRateDTO
    {
        Id = b.BillingRate_Id,
        District = b.District,
        ServiceType = b.ServiceType,
        Rate = b.Rate,
        EffectiveDate = b.Effective,
        IsActive = b.Active ?? false,
        Language = b.Lang
    };
}