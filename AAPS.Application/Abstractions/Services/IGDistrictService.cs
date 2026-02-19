using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IGDistrictService
{
    Task<PagedResult<GDistrictDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<GDistrictDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(GDistrictDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, GDistrictDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
