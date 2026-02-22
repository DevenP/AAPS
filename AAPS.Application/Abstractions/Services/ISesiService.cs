using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface ISesiService
{
    Task<PagedResult<SesiDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<SesiDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(SesiDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, SesiDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<PagedResult<OperationsDTO>> GetOperationsPagedAsync(PagedRequest request, CancellationToken ct = default);
}
