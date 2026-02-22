using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services; // Points to flat folder now
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AAPS.Infrastructure.Services;

public class GDistrictService : IGDistrictService
{
    private readonly IAppDbContext _db;

    public GDistrictService(IAppDbContext db) => _db = db;

    public async Task<Application.Common.Paging.PagedResult<GDistrictDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _db.GDistricts.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<GDistrictDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.GDistricts
            .AsNoTracking()
            .Where(d => d.Dist_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(GDistrictDTO dto, CancellationToken ct = default)
    {
        var entity = new GDistrict { GDist = dto.DistrictCode };
        _db.GDistricts.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Dist_Id;
    }

    public async Task UpdateAsync(int id, GDistrictDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.GDistricts.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.GDist = dto.DistrictCode;
        await _db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.GDistricts.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.GDistricts.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<GDistrict, GDistrictDTO>> ToDTO = d => new GDistrictDTO
    {
        Id = d.Dist_Id,
        DistrictCode = d.GDist

    };
}
