using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IServiceTypeService
{
    Task<PagedResult<ServiceTypeDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<ServiceTypeDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(ServiceTypeDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, ServiceTypeDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
