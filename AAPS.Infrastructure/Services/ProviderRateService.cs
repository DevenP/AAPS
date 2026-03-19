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
        await using var db = _factory.CreateDbContext();
        var entity = new ProviderRate
        {
            Provider_Id = dto.ProviderId,
            ServiceType = dto.ServiceType,
            District = dto.District,
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
        entity.Provider_Id = dto.ProviderId;
        entity.ServiceType = dto.ServiceType;
        entity.District = dto.District;
        entity.Rate = dto.Rate;
        entity.Effective = dto.EffectiveDate;
        entity.Active = dto.IsActive;
        entity.Lang = dto.Language;
        await db.SaveChangesAsync(ct);
    }


    private static readonly Expression<Func<ProviderRate, ProviderRateDTO>> ToDTO = r => new ProviderRateDTO
    {
        Id = r.ProviderRate_Id,
        ProviderId = r.Provider_Id,
        ServiceType = r.ServiceType,
        District = r.District,
        Rate = r.Rate,
        EffectiveDate = r.Effective,
        IsActive = r.Active ?? false,
        Language = r.Lang
    };

}
