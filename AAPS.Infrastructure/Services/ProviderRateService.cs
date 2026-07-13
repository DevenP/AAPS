using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class ProviderRateService : IProviderRateService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProviderRateService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<PagedResult<ProviderRateDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(request.SortBy))
            request = request with { SortBy = "ProviderLastName" };

        await using var db = _factory.CreateDbContext();
        var query = from rate in db.ProviderRates.AsNoTracking()
                    join prov in db.Providers.AsNoTracking()
                      on rate.Provider_Id equals prov.Provider_Id into provJoin
                    from prov in provJoin.DefaultIfEmpty() // Left Join
                    select new ProviderRateDTO
                    {
                        Id = rate.ProviderRate_Id,
                        ProviderId = rate.Provider_Id,
                        // Pulling names from the joined Provider table
                        ProviderFirstName = prov != null ? prov.FirstName : "Unknown",
                        ProviderLastName = prov != null ? prov.LastName : "Unknown",
                        ServiceType = rate.ServiceType,
                        District = rate.District,
                        GroupSize = rate.GroupSize,
                        Rate = rate.Rate,
                        EffectiveDate = rate.Effective,
                        IsActive = rate.Active.HasValue ? rate.Active.Value : false,
                        Language = rate.Lang
                    };

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<ProviderRateDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.ProviderRates
            .AsNoTracking()
            .Where(r => r.ProviderRate_Id == id)
            .Join(db.Providers.AsNoTracking(),
                  rate => rate.Provider_Id,
                  prov => prov.Provider_Id,
                  (rate, prov) => new ProviderRateDTO
                  {
                      Id = rate.ProviderRate_Id,
                      ProviderId = rate.Provider_Id,
                      ProviderFirstName = prov != null ? prov.FirstName : "Unknown",
                      ProviderLastName = prov != null ? prov.LastName : "Unknown",
                      ServiceType = rate.ServiceType,
                      District = rate.District,
                      GroupSize = rate.GroupSize,
                      Rate = rate.Rate,
                      EffectiveDate = rate.Effective,
                      IsActive = rate.Active.HasValue ? rate.Active.Value : false,
                      Language = rate.Lang
                  })
            .FirstOrDefaultAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.ProviderRates.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.ProviderRates.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<int> CreateAsync(ProviderRateDTO dto, CancellationToken ct = default)
    {
        var dupMessage = await CheckActiveDuplicateAsync(dto, null, ct);
        if (dupMessage != null) throw new InvalidOperationException(dupMessage);

        await using var db = _factory.CreateDbContext();
        var entity = new ProviderRate
        {
            Provider_Id = dto.ProviderId,
            ServiceType = dto.ServiceType,
            District = dto.District,
            GroupSize = dto.GroupSize,
            Rate = dto.Rate,
            Effective = dto.EffectiveDate,
            Active = dto.IsActive,
            Lang = dto.Language
        };
        db.ProviderRates.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.ProviderRate_Id;
    }

    public async Task UpdateAsync(int id, ProviderRateDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.ProviderRates.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();

        var dupMessage = await CheckActiveDuplicateAsync(dto, id, ct);
        if (dupMessage != null) throw new InvalidOperationException(dupMessage);

        entity.Provider_Id = dto.ProviderId;
        entity.ServiceType = dto.ServiceType;
        entity.District = dto.District;
        entity.GroupSize = dto.GroupSize;
        entity.Rate = dto.Rate;
        entity.Effective = dto.EffectiveDate;
        entity.Active = dto.IsActive;
        entity.Lang = dto.Language;
        await db.SaveChangesAsync(ct);
    }


    // At most one active rate per provider + service type + district + language + group size.
    // Two active rates on the same key would make rate resolution ambiguous. Inactive rows are
    // left alone - they're history and never resolved. Returns a message when a clash exists.
    public async Task<string?> CheckActiveDuplicateAsync(ProviderRateDTO dto, int? excludeId, CancellationToken ct = default)
    {
        if (!dto.IsActive) return null;

        await using var db = _factory.CreateDbContext();
        var candidates = await db.ProviderRates.AsNoTracking()
            .Where(r => r.Active == true
                     && r.Provider_Id == dto.ProviderId
                     && r.ServiceType == dto.ServiceType)
            .ToListAsync(ct);

        bool clash = candidates.Any(r =>
            r.ProviderRate_Id != excludeId &&
            r.District == dto.District &&
            r.Lang == dto.Language &&
            r.GroupSize == dto.GroupSize);

        if (!clash) return null;

        var sizeText = dto.GroupSize.HasValue ? $"group size {dto.GroupSize}" : "the general group size";
        return $"An active rate already exists for this provider, service type, district, and language at {sizeText}. Edit that rate or mark it inactive before adding another.";
    }

    private static readonly Expression<Func<ProviderRate, ProviderRateDTO>> ToDTO = r => new ProviderRateDTO
    {
        Id = r.ProviderRate_Id,
        ProviderId = r.Provider_Id,
        ServiceType = r.ServiceType,
        District = r.District,
        GroupSize = r.GroupSize,
        Rate = r.Rate,
        EffectiveDate = r.Effective,
        IsActive = r.Active ?? false,
        Language = r.Lang
    };

}
