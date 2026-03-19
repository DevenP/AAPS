using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class EvalService : IEvalService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EvalService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<PagedResult<EvalDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        // Apply global search on the raw entity before projection so EF can
        // translate it against real indexed columns instead of DTO properties.
        var baseQuery = db.Evals.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            baseQuery = baseQuery.Where(ev =>
                (ev.StudentLast != null && ev.StudentLast.Contains(term)) ||
                (ev.StudentFirst != null && ev.StudentFirst.Contains(term)) ||
                (ev.Student_ID != null && ev.Student_ID.Contains(term)) ||
                (ev.Phone != null && ev.Phone.Contains(term)) ||
                (ev.Email != null && ev.Email.Contains(term)) ||
                (ev.ParentLast != null && ev.ParentLast.Contains(term)) ||
                (ev.ParentFirst != null && ev.ParentFirst.Contains(term)) ||
                (ev.District != null && ev.District.Contains(term)) ||
                (ev.Language != null && ev.Language.Contains(term)) ||
                (ev.ServiceType != null && ev.ServiceType.Contains(term)) ||
                (ev.Contact != null && ev.Contact.Contains(term)) ||
                (ev.Memo != null && ev.Memo.Contains(term)) ||
                (ev.Status != null && ev.Status.Contains(term)));
        }

        // Left-join to Providers to surface provider name in the DTO
        var query = from ev in baseQuery
                    join prov in db.Providers.AsNoTracking() on ev.Provider_Id equals prov.Provider_Id into grouping
                    from prov in grouping.DefaultIfEmpty() // This makes it a LEFT JOIN
                    select new EvalDTO
                    {
                        Id = ev.Eval_Id,
                        StudentFirstName = ev.StudentFirst,
                        StudentLastName = ev.StudentLast,
                        StudentId = ev.Student_ID,
                        Phone = ev.Phone,
                        Email = ev.Email,
                        ParentFirstName = ev.ParentFirst,
                        ParentLastName = ev.ParentLast,
                        EvalReceivedDate = ev.EvalReceived,
                        ProviderId = ev.Provider_Id,
                        ProviderFirstName = prov != null ? prov.FirstName : "Unassigned",
                        ProviderLastName = prov != null ? prov.LastName : "Unassigned",
                        AssignedDate = ev.Assigned,
                        ReportReceivedDate = ev.ReportReceived,
                        EvalDate = ev.EvalDate,
                        ReportSubmittedDate = ev.ReportSubmitted,
                        District = ev.District,
                        Language = ev.Language,
                        ServiceType = ev.ServiceType,
                        Contact = ev.Contact,
                        ProviderPaidAmount = ev.pAmount,
                        BillingAmount = ev.bAmount,
                        ProviderPaidDate = ev.pPaid,
                        BillPaidDate = ev.bPaid,
                        BilledDate = ev.Billed,
                        Memo = ev.Memo,
                        AppointmentDate = ev.Appointment,
                        Status = ev.Status,
                    };

        // performSearch: false — search was already applied above on the raw entity
        return await query.ToPagedResultAsync(request, ct, performSearch: false);
    }

    public async Task<EvalDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Evals
            .AsNoTracking()
            .Where(e => e.Eval_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(EvalDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = new Eval
        {
            StudentFirst = dto.StudentFirstName,
            StudentLast = dto.StudentLastName,
            Student_ID = dto.StudentId,
            Phone = dto.Phone,
            Email = dto.Email,
            ParentFirst = dto.ParentFirstName,
            ParentLast = dto.ParentLastName,
            EvalReceived = dto.EvalReceivedDate,
            Provider_Id = dto.ProviderId,
            Assigned = dto.AssignedDate,
            ReportReceived = dto.ReportReceivedDate,
            EvalDate = dto.EvalDate,
            ReportSubmitted = dto.ReportSubmittedDate,
            District = dto.District,
            Language = dto.Language,
            ServiceType = dto.ServiceType,
            Contact = dto.Contact,
            pAmount = dto.ProviderPaidAmount,
            bAmount = dto.BillingAmount,
            pPaid = dto.ProviderPaidDate,
            bPaid = dto.BillPaidDate,
            Billed = dto.BilledDate,
            Memo = dto.Memo,
            Appointment = dto.AppointmentDate,
            Status = dto.Status
        };
        db.Evals.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Eval_Id;
    }

    public async Task UpdateAsync(int id, EvalDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Evals.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException();

        entity.StudentFirst = dto.StudentFirstName;
        entity.StudentLast = dto.StudentLastName;
        entity.Student_ID = dto.StudentId;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.ParentFirst = dto.ParentFirstName;
        entity.ParentLast = dto.ParentLastName;
        entity.EvalReceived = dto.EvalReceivedDate;
        entity.Provider_Id = dto.ProviderId;
        entity.Assigned = dto.AssignedDate;
        entity.ReportReceived = dto.ReportReceivedDate;
        entity.EvalDate = dto.EvalDate;
        entity.ReportSubmitted = dto.ReportSubmittedDate;
        entity.District = dto.District;
        entity.Language = dto.Language;
        entity.ServiceType = dto.ServiceType;
        entity.Contact = dto.Contact;
        entity.pAmount = dto.ProviderPaidAmount;
        entity.bAmount = dto.BillingAmount;
        entity.pPaid = dto.ProviderPaidDate;
        entity.bPaid = dto.BillPaidDate;
        entity.Billed = dto.BilledDate;
        entity.Memo = dto.Memo;
        entity.Appointment = dto.AppointmentDate;
        entity.Status = dto.Status;

        await db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Evals.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.Evals.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<Eval, EvalDTO>> ToDTO = e => new EvalDTO
    {
        Id = e.Eval_Id,
        StudentFirstName = e.StudentFirst,
        StudentLastName = e.StudentLast,
        StudentId = e.Student_ID,
        Phone = e.Phone,
        Email = e.Email,
        ParentFirstName = e.ParentFirst,
        ParentLastName = e.ParentLast,
        EvalReceivedDate = e.EvalReceived,
        ProviderId = e.Provider_Id,
        AssignedDate = e.Assigned,
        ReportReceivedDate = e.ReportReceived,
        EvalDate = e.EvalDate,
        ReportSubmittedDate = e.ReportSubmitted,
        District = e.District,
        Language = e.Language,
        ServiceType = e.ServiceType,
        Contact = e.Contact,
        ProviderPaidAmount = e.pAmount,
        BillingAmount = e.bAmount,
        ProviderPaidDate = e.pPaid,
        BillPaidDate = e.bPaid,
        BilledDate = e.Billed,
        Memo = e.Memo,
        AppointmentDate = e.Appointment,
        Status = e.Status
    };
}
