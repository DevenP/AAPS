using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class ServiceTypeService : IServiceTypeService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ServiceTypeService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<PagedResult<ServiceTypeDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.ServiceTypes.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<ServiceTypeDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.ServiceTypes
            .AsNoTracking()
            .Where(s => s.ServiceType_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(ServiceTypeDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = new ServiceType { ServiceType1 = dto.Name, Eval = dto.IsEvaluation };
        db.ServiceTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.ServiceType_Id;
    }

    public async Task UpdateAsync(int id, ServiceTypeDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.ServiceTypes.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.ServiceType1 = dto.Name;
        entity.Eval = dto.IsEvaluation;
        await db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.ServiceTypes.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.ServiceTypes.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<ServiceType, ServiceTypeDTO>> ToDTO = s => new ServiceTypeDTO
    {
        Id = s.ServiceType_Id,
        Name = s.ServiceType1,
        IsEvaluation = s.Eval ?? false
    };

}
