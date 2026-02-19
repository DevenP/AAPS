using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IMandateService
{
    Task<PagedResult<MandateDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<MandateDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(MandateDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, MandateDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
