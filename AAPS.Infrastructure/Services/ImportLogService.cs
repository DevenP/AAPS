using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class ImportLogService : IImportLogService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ImportLogService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<PagedResult<ImportLogDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.ImportLogs.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<ImportLogDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.ImportLogs
            .AsNoTracking()
            .Where(l => l.Log_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(ImportLogDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = new ImportLog { ImportRecord = dto.ImportRecord, ImportOn = dto.ImportDate, FileName = dto.FileName };
        db.ImportLogs.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Log_Id;
    }

    public async Task UpdateAsync(int id, ImportLogDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.ImportLogs.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.ImportRecord = dto.ImportRecord;
        entity.ImportOn = dto.ImportDate;
        entity.FileName = dto.FileName;
        await db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.ImportLogs.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.ImportLogs.Remove(entity);
            await db.SaveChangesAsync(ct);
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
