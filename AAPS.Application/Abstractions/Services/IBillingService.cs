using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public record GeneratedBillingFile(string FileName, string FilePath, byte[] Bytes);
public record BillingSummary(int Count, decimal TotalBilling, decimal TotalProvider);

public interface IBillingService
{
    Task<PagedResult<BillingRecordDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);
    Task<BillingSummary> GetSummaryAsync(string search, Dictionary<string, string> columnFilters, CancellationToken ct = default);
    Task UpdateBillingDatesAsync(int sesisId, DateTime? billed, DateTime? billedPaidOn, DateTime? providerPaidOn, CancellationToken ct = default);
    Task<List<GeneratedBillingFile>> GenerateBillingFilesAsync(string search, Dictionary<string, string> columnFilters, CancellationToken ct = default);
}
