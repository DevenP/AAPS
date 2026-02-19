using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IImportLogService
{
    Task<PagedResult<ImportLogDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<ImportLogDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(ImportLogDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, ImportLogDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
