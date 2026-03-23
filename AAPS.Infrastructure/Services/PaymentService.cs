using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public PaymentService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<PagedResult<PaymentDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var query = db.Payments.AsNoTracking().Select(p => new PaymentDTO
        {
            VoucherId     = p.Voucher_Id,
            Voucher       = p.Voucher,
            StudentId     = p.Student_ID,
            Ssn           = p.Ssn,
            Provider      = p.Provider,
            DateOfService = p.date_of_Service,
            StartTime     = p.Start_Time,
            VoucherAmount = p.VoucherAmount,
            FileName      = p.FileName,
            RowNumber     = p.RowNumber,
            SesisId       = p.Sesis_Id,
        });

        return await query.ToPagedResultAsync(request, ct);
    }
}
