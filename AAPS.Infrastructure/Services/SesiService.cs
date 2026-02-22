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

public class SesiService : ISesiService
{
    private readonly IAppDbContext _db;

    public SesiService(IAppDbContext db) => _db = db;

    public async Task<Application.Common.Paging.PagedResult<SesiDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _db.Seses.AsNoTracking().Select(ToDTO);

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<SesiDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Seses
            .AsNoTracking()
            .Where(s => s.Sesis_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(SesiDTO dto, CancellationToken ct = default)
    {
        var entity = new Sesi
        {
            Student_ID = dto.StudentId,
            Last_Name = dto.StudentLastName,
            First_Name = dto.StudentFirstName,
            Grade = dto.Grade,
            date_of_Birth = dto.DateOfBirth,
            Home_District = dto.HomeDistrict,
            CSE = dto.Cse,
            CSE_District = dto.CseDistrict,
            Admin_DBN = dto.AdminDbn,
            GDistrict = dto.GDistrict,
            Borough = dto.Borough,
            Mandate_Short = dto.MandateShort,
            Mandatetime_Start = dto.MandateStart,
            Mandated_Max_Group = dto.MandatedMaxGroup,
            Assignment_First_Encounter = dto.AssignmentFirstEncounter,
            Assignment_Claimed = dto.AssignmentClaimed,
            date_of_Service = dto.DateOfService,
            Service_Type = dto.ServiceType,
            Language_Provided = dto.LanguageProvided,
            Session_Type = dto.SessionType,
            Session_Notes = dto.SessionNotes,
            Groupin = dto.Grouping,
            Actual_Size = dto.ActualSize,
            Start_Time = dto.StartTime,
            End_Time = dto.EndTime,
            Duration = dto.Duration,
            Provider_Last_Name = dto.ProviderLastName,
            Provider_First_Name = dto.ProviderFirstName,
            FileName = dto.FileName,
            RowNumber = dto.RowNumber,
            Provider_Id = dto.ProviderId,
            Entry_Id = dto.EntryId,
            bRate = dto.BilledRate,
            bAmount = dto.BilledAmount,
            Billed = dto.BilledDate,
            bPaid = dto.BilledPaidDate,
            pRate = dto.ProviderRate,
            pAmount = dto.ProviderAmount,
            pPaid = dto.ProviderPaidDate,
            Overlap = dto.IsOverlap,
            Voucher = dto.Voucher,
            OverMandate = dto.IsOverMandate,
            OverDuration = dto.IsOverDuration,
            UnderGroup = dto.IsUnderGroup
        };
        _db.Seses.Add(entity); // Assuming 'Seses' is the DbSet name
        await _db.SaveChangesAsync(ct);
        return entity.Sesis_Id;
    }

    public async Task UpdateAsync(int id, SesiDTO dto, CancellationToken ct = default)
    {
        var entity = await _db.Seses.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();

        entity.Student_ID = dto.StudentId;
        entity.Last_Name = dto.StudentLastName;
        entity.First_Name = dto.StudentFirstName;
        entity.Grade = dto.Grade;
        entity.date_of_Birth = dto.DateOfBirth;
        entity.Home_District = dto.HomeDistrict;
        entity.CSE = dto.Cse;
        entity.CSE_District = dto.CseDistrict;
        entity.Admin_DBN = dto.AdminDbn;
        entity.GDistrict = dto.GDistrict;
        entity.Borough = dto.Borough;
        entity.Mandate_Short = dto.MandateShort;
        entity.Mandatetime_Start = dto.MandateStart;
        entity.Mandated_Max_Group = dto.MandatedMaxGroup;
        entity.Assignment_First_Encounter = dto.AssignmentFirstEncounter;
        entity.Assignment_Claimed = dto.AssignmentClaimed;
        entity.date_of_Service = dto.DateOfService;
        entity.Service_Type = dto.ServiceType;
        entity.Language_Provided = dto.LanguageProvided;
        entity.Session_Type = dto.SessionType;
        entity.Session_Notes = dto.SessionNotes;
        entity.Groupin = dto.Grouping;
        entity.Actual_Size = dto.ActualSize;
        entity.Start_Time = dto.StartTime;
        entity.End_Time = dto.EndTime;
        entity.Duration = dto.Duration;
        entity.Provider_Last_Name = dto.ProviderLastName;
        entity.Provider_First_Name = dto.ProviderFirstName;
        entity.FileName = dto.FileName;
        entity.RowNumber = dto.RowNumber;
        entity.Provider_Id = dto.ProviderId;
        entity.Entry_Id = dto.EntryId;
        entity.bRate = dto.BilledRate;
        entity.bAmount = dto.BilledAmount;
        entity.Billed = dto.BilledDate;
        entity.bPaid = dto.BilledPaidDate;
        entity.pRate = dto.ProviderRate;
        entity.pAmount = dto.ProviderAmount;
        entity.pPaid = dto.ProviderPaidDate;
        entity.Overlap = dto.IsOverlap;
        entity.Voucher = dto.Voucher;
        entity.OverMandate = dto.IsOverMandate;
        entity.OverDuration = dto.IsOverDuration;
        entity.UnderGroup = dto.IsUnderGroup;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Seses.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _db.Seses.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<Application.Common.Paging.PagedResult<OperationsDTO>> GetOperationsPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        // 1. Get the single 'best' Assignment ID for each EntryId to prevent row doubling
        var topAssignments = _db.VendorPortals.AsNoTracking()
            .GroupBy(v => v.Entry_Id)
            .Select(g => new {
                EntryId = g.Key,
                AssignId = g.OrderBy(x => x.VendorPortal_Id).Select(x => x.Assign_Id).FirstOrDefault()
            });

        // 2. Main Operations Query
        var query = from s in _db.Seses.AsNoTracking()
                    join m in _db.Mandates.AsNoTracking() on s.Entry_Id equals m.Entry_Id into mGroup
                    from m in mGroup.DefaultIfEmpty()
                    join p in _db.Providers.AsNoTracking() on s.Provider_Id equals p.Provider_Id into pGroup
                    from p in pGroup.DefaultIfEmpty()
                    join v in topAssignments on s.Entry_Id equals v.EntryId into vGroup
                    from v in vGroup.DefaultIfEmpty()

                    select new OperationsDTO
                    {
                        Id = s.Sesis_Id,
                        StudentId = s.Student_ID,
                        StudentLastName = s.Last_Name,
                        StudentFirstName = s.First_Name,
                        DateOfService = s.date_of_Service,
                        StartTime = s.Start_Time,
                        EndTime = s.End_Time,
                        Duration = s.Duration,
                        ServiceType = s.Service_Type,
                        BilledRate = s.bRate,
                        ProviderRate = s.pRate,
                        BilledDate = s.Billed,
                        BilledPaidDate = s.bPaid,
                        ProviderId = s.Provider_Id,
                        EntryId = s.Entry_Id,

                        // Alerts / Flags (The 'Alerts' Icons logic)
                        MandateFlag = s.Entry_Id == null,
                        ProviderFlag = s.Provider_Id == null,
                        BRateFlag = s.bRate == null,
                        PRateFlag = s.pRate == null,
                        AssignFlag = v == null || string.IsNullOrEmpty(v.AssignId),

                        // Logic Flags (from your DB bits)
                        IsOverDuration = s.OverDuration.HasValue ? s.OverDuration.Value : false,
                        IsOverlap = s.Overlap.HasValue ? s.Overlap.Value : false,
                        IsOverMandate = s.OverMandate.HasValue ? s.OverMandate.Value : false,
                        IsUnderGroup = s.UnderGroup.HasValue ? s.UnderGroup.Value : false,

                        // Joined Info
                        ProviderLastName = s.Provider_Last_Name,
                        ProviderFirstName = s.Provider_First_Name,
                        AssignId = v != null ? v.AssignId : null,
                        MandateStart = m != null ? m.MandateStart : null,


                        // Masked SSN
                        Ssn = (p != null && p.Ssn != null && p.Ssn.Length >= 4)
                              ? "***-**-" + p.Ssn.Substring(p.Ssn.Length - 4)
                              : (p.Ssn ?? "N/A"),
                              
                    };

        return await query.ToPagedResultAsync(request, ct);
    }

    //public async Task<Application.Common.Paging.PagedResult<OperationsDTO>> GetOperationsPagedAsync(PagedRequest request, CancellationToken ct = default)
    //{
    //    var query = from s in _db.Seses.AsNoTracking()
    //                join m in _db.Mandates.AsNoTracking() on s.Entry_Id equals m.Entry_Id into mJoin
    //                from m in mJoin.DefaultIfEmpty()
    //                join p in _db.Providers.AsNoTracking() on s.Provider_Id equals p.Provider_Id into pJoin
    //                from p in pJoin.DefaultIfEmpty()
    //                    // Simplified VendorPortal logic
    //                join v in _db.VendorPortals.AsNoTracking() on s.Entry_Id equals v.Entry_Id into vJoin
    //                from v in vJoin.DefaultIfEmpty()

    //                select new OperationsDTO
    //                {
    //                    // Map Sesis base fields
    //                    Id = s.Sesis_Id,
    //                    StudentId = s.Student_ID,
    //                    DateOfService = s.date_of_Service,
    //                    StartTime = s.Start_Time,
    //                    IsOverDuration = s.OverDuration.HasValue ? s.OverDuration.Value : false,
    //                    IsOverlap = s.Overlap.HasValue ? s.Overlap.Value : false,
    //                    IsOverMandate = s.OverMandate.HasValue ? s.OverMandate.Value : false,
    //                    IsUnderGroup = s.UnderGroup.HasValue ? s.UnderGroup.Value : false,
    //                    StudentFirstName = s.First_Name,
    //                    StudentLastName = s.Last_Name,


    //                    // ... include other SesiDTO fields ...

    //                    // Flags from Stored Proc
    //                    MandateFlag = s.Entry_Id == null,
    //                    ProviderFlag = s.Provider_Id == null,
    //                    BRateFlag = s.bRate == null,
    //                    PRateFlag = s.pRate == null,
    //                    AssignFlag = v == null || v.Assign_Id == null,

    //                    // Joined Data
    //                    AssignId = v != null ? v.Assign_Id : null,
    //                    MandateStart = m != null ? m.MandateStart : null,
    //                    FullAddress = p != null ? $"{p.Address}, {p.City}, {p.State} {p.Zipcode}" : null,
    //                    Ssn = p != null && p.Ssn.Length >= 4 ? "***-**-" + p.Ssn.Substring(p.Ssn.Length - 4) : ""
    //                };

    //    return await query.ToPagedResultAsync(request, ct);
    //}


    private static readonly Expression<Func<Sesi, SesiDTO>> ToDTO = s => new SesiDTO
    {
        Id = s.Sesis_Id,
        StudentId = s.Student_ID,
        StudentLastName = s.Last_Name,
        StudentFirstName = s.First_Name,
        Grade = s.Grade,
        DateOfBirth = s.date_of_Birth,
        HomeDistrict = s.Home_District,
        Cse = s.CSE,
        CseDistrict = s.CSE_District,
        AdminDbn = s.Admin_DBN,
        GDistrict = s.GDistrict,
        Borough = s.Borough,
        MandateShort = s.Mandate_Short,
        MandateStart = s.Mandatetime_Start,
        MandatedMaxGroup = s.Mandated_Max_Group,
        AssignmentFirstEncounter = s.Assignment_First_Encounter,
        AssignmentClaimed = s.Assignment_Claimed,
        DateOfService = s.date_of_Service,
        ServiceType = s.Service_Type,
        LanguageProvided = s.Language_Provided,
        SessionType = s.Session_Type,
        SessionNotes = s.Session_Notes,
        Grouping = s.Groupin,
        ActualSize = s.Actual_Size,
        StartTime = s.Start_Time,
        EndTime = s.End_Time,
        Duration = s.Duration,
        ProviderLastName = s.Provider_Last_Name,
        ProviderFirstName = s.Provider_First_Name,
        FileName = s.FileName,
        RowNumber = s.RowNumber,
        ProviderId = s.Provider_Id,
        EntryId = s.Entry_Id,
        BilledRate = s.bRate,
        BilledAmount = s.bAmount,
        BilledDate = s.Billed,
        BilledPaidDate = s.bPaid,
        ProviderRate = s.pRate,
        ProviderAmount = s.pAmount,
        ProviderPaidDate = s.pPaid,
        IsOverlap = s.Overlap ?? false,
        Voucher = s.Voucher,
        IsOverMandate = s.OverMandate ?? false,
        IsOverDuration = s.OverDuration ?? false,
        IsUnderGroup = s.UnderGroup ?? false
    };

}
