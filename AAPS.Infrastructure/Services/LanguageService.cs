using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class LanguageService : ILanguageService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public LanguageService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<PagedResult<LanguageDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.Languages.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<LanguageDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Languages
            .AsNoTracking()
            .Where(l => l.Language_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(LanguageDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = new Language { Lang = dto.Name, LangCode = dto.Code };
        db.Languages.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Language_Id;
    }

    public async Task UpdateAsync(int id, LanguageDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Languages.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.Lang = dto.Name;
        entity.LangCode = dto.Code;
        await db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Languages.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.Languages.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<Language, LanguageDTO>> ToDTO = l => new LanguageDTO
    {
        Id = l.Language_Id,
        Name = l.Lang,
        Code = l.LangCode
    };
}
