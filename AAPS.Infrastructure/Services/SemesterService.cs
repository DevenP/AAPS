using AAPS.Application.Abstractions.Services;
using AAPS.Application.DTO;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;

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
            .Select(s => new SemesterDTO
            {
                Id = s.Semester_Id,
                Code = s.Code,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            })
            .ToListAsync(ct);
    }
}
