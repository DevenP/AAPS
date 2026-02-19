using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AAPS.Infrastructure.Services;

public class ImportLogService : IImportLogService
{
    private readonly IAppDbContext _db;

    public ImportLogService(IAppDbContext db) => _db = db;

    public async Task<Application.Common.Paging.PagedResult<ImportLogDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _db.ImportLogs.AsNoTracking().Select(ToDTO);

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

    public async Task<ImportLogDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.ImportLogs
            .AsNoTracking()
            .Where(l => l.Log_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(ImportLogDTO dto, CancellationToken ct = default)
    {
        var entity = new ImportLog { ImportRecord = dto.ImportRecord, ImportOn = dto.ImportDate, FileName = dto.FileName };
        _db.ImportLogs.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Log_Id;
    }

    public async Task UpdateAsync(int id, ImportLogDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.ImportLogs.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.ImportRecord = dto.ImportRecord;
        entity.ImportOn = dto.ImportDate;
        entity.FileName = dto.FileName;
        await _db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.ImportLogs.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.ImportLogs.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<ImportLog, ImportLogDTO>> ToDTO = l => new ImportLogDTO
    {
        Id = l.Log_Id,
        ImportRecord = l.ImportRecord,
        ImportDate = l.ImportOn,
        FileName = l.FileName
    };
}
