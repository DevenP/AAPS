using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface ISemesterService
{
    // All semesters ordered by start date (oldest first).
    Task<List<SemesterDTO>> GetAllAsync(CancellationToken ct = default);
}
