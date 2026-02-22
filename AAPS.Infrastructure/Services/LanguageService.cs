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

public class LanguageService : ILanguageService
{
    private readonly IAppDbContext _db;

    public LanguageService(IAppDbContext db) => _db = db;

    public async Task<Application.Common.Paging.PagedResult<LanguageDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _db.Languages.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<LanguageDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Languages
            .AsNoTracking()
            .Where(l => l.Language_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(LanguageDTO dto, CancellationToken ct = default)
    {
        var entity = new Language { Lang = dto.Name, LangCode = dto.Code };
        _db.Languages.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Language_Id;
    }

    public async Task UpdateAsync(int id, LanguageDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.Languages.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.Lang = dto.Name;
        entity.LangCode = dto.Code;
        await _db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Languages.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.Languages.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<Language, LanguageDTO>> ToDTO = l => new LanguageDTO
    {
        Id = l.Language_Id,
        Name = l.Lang,
        Code = l.LangCode
    };
}
