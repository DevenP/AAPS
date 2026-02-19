using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class EvalService : IEvalService
{
    private readonly IAppDbContext _db;

    public EvalService(IAppDbContext db)
    {
        _db = db;
    }

    //public async Task<Application.Common.Paging.PagedResult<EvalDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    //{
    //    var query = _db.Evals.AsNoTracking().Select(ToDTO);

    //    if (request.ColumnFilters?.Any() == true)
    //    {
    //        foreach (var col in request.ColumnFilters)
    //        {
    //            if (string.IsNullOrWhiteSpace(col.Value)) continue;

    //            query = query.Where($"{col.Key}.Contains(@0)", col.Value);
    //        }
    //    }

    //    return await query.ToPagedResultAsync(request, ct);
    //}

    public async Task<Application.Common.Paging.PagedResult<EvalDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        // 1. Start the Manual Left Join
        var query = from e in _db.Evals
                    join p in _db.Providers on e.Provider_Id equals p.Provider_Id into grouping
                    from p in grouping.DefaultIfEmpty() // This makes it a LEFT JOIN
                    select new EvalDTO
                    {
                        Id = e.Eval_Id,
                        // Manual Concatenation since there's no navigation property
                        ProviderName = p != null ? p.LastName + ", " + p.FirstName : "Unassigned",
                        StudentFirstName = e.StudentFirst,
                        StudentLastName = e.StudentLast,
                        StudentId = e.Student_ID,
                        Status = e.Status,
                        EvalReceivedDate = e.EvalReceived,
                        ReportReceivedDate = e.ReportReceived,
                        BilledDate = e.Billed,
                        BilledPaidDate = e.bPaid,
                        ProviderPaidDate = e.pPaid,
                        AssignedDate = e.Assigned,
                        District = e.District,
                        ServiceType = e.ServiceType
                        // ... map other fields as needed
                    };

        // 2. Handle Advanced Column Filters (Now works on ProviderName too!)
        if (request.ColumnFilters?.Any() == true)
        {
            foreach (var col in request.ColumnFilters)
            {
                if (string.IsNullOrWhiteSpace(col.Value)) continue;

                // Check for the "null/notnull" keywords we discussed
                if (col.Value == "null") query = query.Where($"{col.Key} == null");
                else if (col.Value == "notnull") query = query.Where($"{col.Key} != null");
                else query = query.Where($"{col.Key}.Contains(@0)", col.Value);
            }
        }

        // 3. Hand off to the Generic Extension
        return await query.ToPagedResultAsync(request, ct);
    }



    public async Task<EvalDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Evals
            .AsNoTracking()
            .Where(e => e.Eval_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(EvalDTO dto, CancellationToken ct = default)
    {
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
            pAmount = dto.ProviderAmount,
            bAmount = dto.BilledAmount,
            pPaid = dto.ProviderPaidDate,
            bPaid = dto.BilledPaidDate,
            Billed = dto.BilledDate,
            Memo = dto.Memo,
            Appointment = dto.AppointmentDate,
            Status = dto.Status
        };
        _db.Evals.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Eval_Id;
    }

    public async Task UpdateAsync(int id, EvalDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.Evals.FindAsync(new object[] { id }, ct)
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
        entity.pAmount = dto.ProviderAmount;
        entity.bAmount = dto.BilledAmount;
        entity.pPaid = dto.ProviderPaidDate;
        entity.bPaid = dto.BilledPaidDate;
        entity.Billed = dto.BilledDate;
        entity.Memo = dto.Memo;
        entity.Appointment = dto.AppointmentDate;
        entity.Status = dto.Status;

        await _db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Evals.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.Evals.Remove(entity);
            await _db.SaveChangesAsync(ct);
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
        ProviderAmount = e.pAmount,
        BilledAmount = e.bAmount,
        ProviderPaidDate = e.pPaid,
        BilledPaidDate = e.bPaid,
        BilledDate = e.Billed,
        Memo = e.Memo,
        AppointmentDate = e.Appointment,
        Status = e.Status
    };
}
