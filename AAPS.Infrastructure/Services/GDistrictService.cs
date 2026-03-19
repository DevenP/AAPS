using AAPS.Application.Abstractions.Services; // Points to flat folder now
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace AAPS.Infrastructure.Services;

public class GDistrictService : IGDistrictService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public GDistrictService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<PagedResult<GDistrictDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.GDistricts.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<GDistrictDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.GDistricts
            .AsNoTracking()
            .Where(d => d.Dist_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(GDistrictDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = new GDistrict { GDist = dto.DistrictCode };
        db.GDistricts.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Dist_Id;
    }

    public async Task UpdateAsync(int id, GDistrictDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.GDistricts.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.GDist = dto.DistrictCode;
        await db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.GDistricts.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.GDistricts.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<GDistrict, GDistrictDTO>> ToDTO = d => new GDistrictDTO
    {
        Id = d.Dist_Id,
        DistrictCode = d.GDist

    };
}
