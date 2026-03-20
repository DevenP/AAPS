using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IBillingService
{
    Task<PagedResult<BillingRecordDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);
    Task UpdateBillingDatesAsync(int sesisId, DateTime? billed, DateTime? billedPaidOn, DateTime? providerPaidOn, CancellationToken ct = default);
}
