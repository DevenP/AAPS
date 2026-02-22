using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IProviderService
{
    Task<PagedResult<ProviderDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<ProviderDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> AddAsync(ProviderDTO dto, CancellationToken ct = default);

    Task<bool> UpdateAsync(ProviderDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
