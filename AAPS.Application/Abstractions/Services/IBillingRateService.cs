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

    /// <summary>
    /// Returns the active rate for a Service Type / District / Language combination - used to
    /// auto-fill evaluation billing amounts (evals are keyed the same way as session rates).
    /// Returns null if no active rate exists for the combination.
    /// </summary>
    Task<decimal?> GetRateAsync(string? serviceType, string? district, string? language, CancellationToken ct = default);
}
