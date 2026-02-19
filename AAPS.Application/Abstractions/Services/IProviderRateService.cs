using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IProviderRateService
{
    Task<PagedResult<ProviderRateDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<ProviderRateDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(ProviderRateDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, ProviderRateDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
