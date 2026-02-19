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

public class ServiceTypeService : IServiceTypeService
{
    private readonly IAppDbContext _db;

    public ServiceTypeService(IAppDbContext db) => _db = db;

    public async Task<Application.Common.Paging.PagedResult<ServiceTypeDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _db.ServiceTypes.AsNoTracking().Select(ToDTO);

        if (request.ColumnFilters?.Any() == true)
        {
            foreach (var col in request.ColumnFilters)
            {
                if (string.IsNullOrWhiteSpace(col.Value)) continue;
                query = query.Where($"{col.Key}.Contains(@0)", col.Value);
            }
        }

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<ServiceTypeDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.ServiceTypes
            .AsNoTracking()
            .Where(s => s.ServiceType_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(ServiceTypeDTO dto, CancellationToken ct = default)
    {
        var entity = new ServiceType { ServiceType1 = dto.Name, Eval = dto.IsEvaluation };
        _db.ServiceTypes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.ServiceType_Id;
    }

    public async Task UpdateAsync(int id, ServiceTypeDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.ServiceTypes.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.ServiceType1 = dto.Name;
        entity.Eval = dto.IsEvaluation;
        await _db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.ServiceTypes.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.ServiceTypes.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<ServiceType, ServiceTypeDTO>> ToDTO = s => new ServiceTypeDTO
    {
        Id = s.ServiceType_Id,
        Name = s.ServiceType1,
        IsEvaluation = s.Eval ?? false
    };

}
