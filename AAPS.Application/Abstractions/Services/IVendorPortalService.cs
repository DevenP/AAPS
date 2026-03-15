using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IVendorPortalService
{
    Task<PagedResult<VendorPortalDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<VendorPortalDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(VendorPortalDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, VendorPortalDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task DeleteManyAsync(IEnumerable<int> ids, CancellationToken ct = default);

    Task ReplaceEntryIdAsync(IEnumerable<int> ids, int newEntryId, CancellationToken ct = default);
}