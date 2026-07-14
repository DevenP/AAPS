using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface ISemesterService
{
    // All semesters ordered by start date (oldest first).
    Task<List<SemesterDTO>> GetAllAsync(CancellationToken ct = default);

    Task<PagedResult<SemesterDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<SemesterDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(SemesterDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, SemesterDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    // Returns a message when the semester clashes with an existing one (duplicate code or
    // overlapping date range), or null when it's clear. Pass the row's own id as excludeId on edit.
    Task<string?> CheckConflictAsync(SemesterDTO dto, int? excludeId, CancellationToken ct = default);
}
