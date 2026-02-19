using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IEvalService
{
    Task<PagedResult<EvalDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<EvalDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(EvalDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, EvalDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
