using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface ISesiService
{
    Task<PagedResult<SesiDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task<SesiDTO?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(SesiDTO dto, CancellationToken ct = default);

    Task UpdateAsync(int id, SesiDTO dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<PagedResult<OperationsDTO>> GetOperationsPagedAsync(PagedRequest request, CancellationToken ct = default);

    Task BulkUpdateAsync(List<int> ids, OperationEditDTO dto, CancellationToken ct = default);

    Task BulkUnlinkApprovalIdAsync(List<int> ids, CancellationToken ct = default);

    /// <summary>Returns the number of records actually deleted (skips any with Entry_Id set).</summary>
    Task<int> BulkDeleteProviderBillingAsync(List<int> ids, CancellationToken ct = default);

    /// <summary>CSE=15: marks VoucherBalancePaid = now (only if currently null)</summary>
    Task BulkMarkVoucherBalancePaidAsync(List<int> ids, CancellationToken ct = default);

    /// <summary>CSE=16: clears VoucherBalancePaid (only where VoucherAmount &lt;&gt; bAmount and currently set)</summary>
    Task BulkUnmarkVoucherBalancePaidAsync(List<int> ids, CancellationToken ct = default);

    /// <summary>Mirrors Mandates_By_Osis: all mandates for a student with aggregated Assign IDs per mandate.</summary>
    Task<List<OsisMandateDTO>> GetOsisMandatesAsync(string studentId, CancellationToken ct = default);
}
