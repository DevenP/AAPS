using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class SemesterService : ISemesterService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public SemesterService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<SemesterDTO>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Semesters
            .AsNoTracking()
            .OrderBy(s => s.StartDate)
            .Select(ToDTO)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<SemesterDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.Semesters.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<SemesterDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Semesters
            .AsNoTracking()
            .Where(s => s.Semester_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(SemesterDTO dto, CancellationToken ct = default)
    {
        var conflict = await CheckConflictAsync(dto, null, ct);
        if (conflict != null) throw new InvalidOperationException(conflict);

        await using var db = _factory.CreateDbContext();
        var entity = new Semester
        {
            Code = dto.Code,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };
        db.Semesters.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Semester_Id;
    }

    public async Task UpdateAsync(int id, SemesterDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var conflict = await CheckConflictAsync(dto, id, ct);
        if (conflict != null) throw new InvalidOperationException(conflict);

        var entity = await db.Semesters.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.Code = dto.Code;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        await db.SaveChangesAsync(ct);
    }

    // A semester's code must be unique and its date range must not overlap any other semester's,
    // otherwise "current semester" resolution and the date filter become ambiguous. Gaps between
    // terms are fine. Returns a message describing the clash, or null when clear.
    public async Task<string?> CheckConflictAsync(SemesterDTO dto, int? excludeId, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var others = await db.Semesters.AsNoTracking()
            .Where(s => excludeId == null || s.Semester_Id != excludeId.Value)
            .ToListAsync(ct);

        var code = (dto.Code ?? "").Trim();
        if (others.Any(s => string.Equals((s.Code ?? "").Trim(), code, StringComparison.OrdinalIgnoreCase)))
            return $"A semester with the code \"{code}\" already exists. Codes must be unique.";

        // Two inclusive ranges overlap when each starts on or before the other ends.
        var overlap = others.FirstOrDefault(s => dto.StartDate <= s.EndDate && s.StartDate <= dto.EndDate);
        if (overlap != null)
            return $"These dates overlap the \"{overlap.Code}\" semester ({overlap.StartDate:MM/dd/yyyy}–{overlap.EndDate:MM/dd/yyyy}). Semester date ranges can't overlap.";

        return null;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Semesters.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.Semesters.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<Semester, SemesterDTO>> ToDTO = s => new SemesterDTO
    {
        Id = s.Semester_Id,
        Code = s.Code,
        StartDate = s.StartDate,
        EndDate = s.EndDate
    };
}
