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

public class MandateService : IMandateService
{
    private readonly IAppDbContext _db;

    public MandateService(IAppDbContext db) => _db = db;

    public async Task<Application.Common.Paging.PagedResult<MandateDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _db.Mandates.AsNoTracking().Select(ToDTO);

        if (request.ColumnFilters?.Any() == true)
        {
            foreach (var col in request.ColumnFilters)
            {
                if (string.IsNullOrWhiteSpace(col.Value)) continue;
                query = query.Where($"{col.Key}.Contains(@0)", col.Value);
            }
        }

        return await query.ToPagedResultAsync(request, ct);
    }


    public async Task<MandateDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Mandates
            .AsNoTracking()
            .Where(m => m.Entry_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(MandateDTO dto, CancellationToken ct = default)
    {
        var entity = new Mandate
        {
            Conf_Date = dto.ConferenceDate,
            Student_ID = dto.StudentId,
            Last_Name = dto.LastName,
            First_Name = dto.FirstName,
            Home_District = dto.HomeDistrict,
            CSE = dto.Cse,
            CSE_District = dto.CseDistrict,
            Grade = dto.Grade,
            Date_of_Birth = dto.DateOfBirth,
            Admin_DBN = dto.AdminDbn,
            D75 = dto.D75,
            Service_Type = dto.ServiceType,
            Lang = dto.Language,
            Grp_Size = dto.GroupSize,
            Dur = dto.Duration,
            Service_Location = dto.ServiceLocation,
            Remaining_Freq = dto.RemainingFrequency,
            Provider = dto.Provider,
            First_Attend_Date = dto.FirstAttendDate,
            Mandate_ID = dto.MandateId,
            Primary_Contact_Phone_1 = dto.PrimaryPhone1,
            Primary_Contact_Phone_2 = dto.PrimaryPhone2,
            MandateStart = dto.MandateStart,
            MandateEnd = dto.MandateEnd,
            FileName = dto.FileName,
            RowNumber = dto.RowNumber,
            Service_Start_Date = dto.ServiceStartDate
        };
        _db.Mandates.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Entry_Id;
    }

    public async Task UpdateAsync(int id, MandateDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.Mandates.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();
        entity.Conf_Date = dto.ConferenceDate;
        entity.Student_ID = dto.StudentId;
        entity.Last_Name = dto.LastName;
        entity.First_Name = dto.FirstName;
        entity.Home_District = dto.HomeDistrict;
        entity.CSE = dto.Cse;
        entity.CSE_District = dto.CseDistrict;
        entity.Grade = dto.Grade;
        entity.Date_of_Birth = dto.DateOfBirth;
        entity.Admin_DBN = dto.AdminDbn;
        entity.D75 = dto.D75;
        entity.Service_Type = dto.ServiceType;
        entity.Lang = dto.Language;
        entity.Grp_Size = dto.GroupSize;
        entity.Dur = dto.Duration;
        entity.Service_Location = dto.ServiceLocation;
        entity.Remaining_Freq = dto.RemainingFrequency;
        entity.Provider = dto.Provider;
        entity.First_Attend_Date = dto.FirstAttendDate;
        entity.Mandate_ID = dto.MandateId;
        entity.Primary_Contact_Phone_1 = dto.PrimaryPhone1;
        entity.Primary_Contact_Phone_2 = dto.PrimaryPhone2;
        entity.MandateStart = dto.MandateStart;
        entity.MandateEnd = dto.MandateEnd;
        entity.FileName = dto.FileName;
        entity.RowNumber = dto.RowNumber;
        entity.Service_Start_Date = dto.ServiceStartDate;
        await _db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Mandates.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.Mandates.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static readonly Expression<Func<Mandate, MandateDTO>> ToDTO = m => new MandateDTO
    {
        Id = m.Entry_Id,
        ConferenceDate = m.Conf_Date,
        StudentId = m.Student_ID,
        LastName = m.Last_Name,
        FirstName = m.First_Name,
        HomeDistrict = m.Home_District,
        Cse = m.CSE,
        CseDistrict = m.CSE_District,
        Grade = m.Grade,
        DateOfBirth = m.Date_of_Birth,
        AdminDbn = m.Admin_DBN,
        D75 = m.D75,
        ServiceType = m.Service_Type,
        Language = m.Lang,
        GroupSize = m.Grp_Size,
        Duration = m.Dur,
        ServiceLocation = m.Service_Location,
        RemainingFrequency = m.Remaining_Freq,
        Provider = m.Provider,
        FirstAttendDate = m.First_Attend_Date,
        MandateId = m.Mandate_ID,
        PrimaryPhone1 = m.Primary_Contact_Phone_1,
        PrimaryPhone2 = m.Primary_Contact_Phone_2,
        MandateStart = m.MandateStart,
        MandateEnd = m.MandateEnd,
        FileName = m.FileName,
        RowNumber = m.RowNumber,
        ServiceStartDate = m.Service_Start_Date
    };

}
