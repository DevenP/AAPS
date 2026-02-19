using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IProviderContactService
{
    Task<PagedResult<ProviderContactDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<ProviderContactDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(ProviderContactDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, ProviderContactDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
