using AAPS.Application.Common.Paging;

namespace AAPS.Application.VendorPortals;

public interface IVendorPortalQueryService
{
    Task<PagedResult<Dictionary<string, object?>>> GetAsync(
        PagedRequest request,
        CancellationToken ct = default);
}
