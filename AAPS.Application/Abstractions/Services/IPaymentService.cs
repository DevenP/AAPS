using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IPaymentService
{
    Task<PagedResult<PaymentDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);
}
