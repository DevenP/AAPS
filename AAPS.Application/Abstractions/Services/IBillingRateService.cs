using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public record BillingRateUsage(int SesiCount, int EvalCount);

public interface IBillingRateService
{
    Task<PagedResult<BillingRateDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<BillingRateDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(BillingRateDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, BillingRateDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<BillingRateUsage> GetUsageCountAsync(int id, CancellationToken ct = default);
}
