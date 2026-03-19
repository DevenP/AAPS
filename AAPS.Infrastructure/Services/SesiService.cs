using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class SesiService : ISesiService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public SesiService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<PagedResult<SesiDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        // Apply global search on the raw entity before projection so EF can
        // translate it against real indexed columns instead of DTO properties.
        var baseQuery = db.Seses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            baseQuery = baseQuery.Where(s =>
                (s.Student_ID != null && s.Student_ID.Contains(term)) ||
                (s.Last_Name != null && s.Last_Name.Contains(term)) ||
                (s.First_Name != null && s.First_Name.Contains(term)) ||
                (s.Grade != null && s.Grade.Contains(term)) ||
                (s.Home_District != null && s.Home_District.Contains(term)) ||
                (s.CSE != null && s.CSE.Contains(term)) ||
                (s.CSE_District != null && s.CSE_District.Contains(term)) ||
                (s.Admin_DBN != null && s.Admin_DBN.Contains(term)) ||
                (s.GDistrict != null && s.GDistrict.Contains(term)) ||
                (s.Borough != null && s.Borough.Contains(term)) ||
                (s.Mandate_Short != null && s.Mandate_Short.Contains(term)) ||
                (s.Mandated_Max_Group != null && s.Mandated_Max_Group.Contains(term)) ||
                (s.Assignment_Claimed != null && s.Assignment_Claimed.Contains(term)) ||
                (s.Service_Type != null && s.Service_Type.Contains(term)) ||
                (s.Language_Provided != null && s.Language_Provided.Contains(term)) ||
                (s.Session_Type != null && s.Session_Type.Contains(term)) ||
                (s.Session_Notes != null && s.Session_Notes.Contains(term)) ||
                (s.Groupin != null && s.Groupin.Contains(term)) ||
                (s.Actual_Size != null && s.Actual_Size.Contains(term)) ||
                (s.Start_Time != null && s.Start_Time.Contains(term)) ||
                (s.End_Time != null && s.End_Time.Contains(term)) ||
                (s.Duration != null && s.Duration.Contains(term)) ||
                (s.Provider_Last_Name != null && s.Provider_Last_Name.Contains(term)) ||
                (s.Provider_First_Name != null && s.Provider_First_Name.Contains(term)) ||
                (s.FileName != null && s.FileName.Contains(term)) ||
                (s.Voucher != null && s.Voucher.Contains(term)));
        }

        var query = baseQuery.Select(ToDTO);

        // performSearch: false — search was already applied above on the raw entity
        return await query.ToPagedResultAsync(request, ct, performSearch: false);
    }

    public async Task<SesiDTO?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Seses
            .AsNoTracking()
            .Where(s => s.Sesis_Id == id)
            .Select(ToDTO)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateAsync(SesiDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
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
        db.Seses.Add(entity); // Assuming 'Seses' is the DbSet name
        await db.SaveChangesAsync(ct);
        return entity.Sesis_Id;
    }

    public async Task UpdateAsync(int id, SesiDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Seses.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();

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

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Seses.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            db.Seses.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<PagedResult<OperationsDTO>> GetOperationsPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        // Mirrors the stored proc's two-step VendorPortal join:
        //
        //   Step 1 — find the MIN(VendorPortal_Id) per Entry_Id + pSsn combo
        //   Step 2 — join VendorPortal on that specific VendorPortal_Id
        //   Step 3 — when joining to Sesis, also verify pSsn matches the provider's SSN
        //            (proc strips dashes: LEFT(Ssn,3)+SUBSTRING(Ssn,5,2)+RIGHT(Ssn,4))
        //
        // This prevents an assignment from one provider bleeding onto a different
        // provider's session rows that happen to share the same Entry_Id.

        // Step 1: get the winning VendorPortal_Id per (Entry_Id, pSsn)
        var topAssignByEntryAndSsn = db.VendorPortals.AsNoTracking()
            .Where(v => v.Entry_Id != null && v.pSsn != null)
            .GroupBy(v => new { v.Entry_Id, v.pSsn })
            .Select(g => new
            {
                EntryId = g.Key.Entry_Id,
                ProvSsn = g.Key.pSsn,
                TopId = g.Min(x => x.VendorPortal_Id)
            });

        // Step 2: join back to get the Assign_Id for that winning row
        var topAssignments =
            from t in topAssignByEntryAndSsn
            join v in db.VendorPortals.AsNoTracking() on t.TopId equals v.VendorPortal_Id
            select new
            {
                t.EntryId,
                t.ProvSsn,
                v.Assign_Id
            };

        // Apply global search on the raw Seses table BEFORE joins/projection
        // so EF hits real indexed columns instead of DTO projections
        var baseQuery = db.Seses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            baseQuery = baseQuery.Where(s =>
                (s.Student_ID != null && s.Student_ID.Contains(term)) ||
                (s.Last_Name != null && s.Last_Name.Contains(term)) ||
                (s.First_Name != null && s.First_Name.Contains(term)) ||
                (s.Provider_Last_Name != null && s.Provider_Last_Name.Contains(term)) ||
                (s.Provider_First_Name != null && s.Provider_First_Name.Contains(term)) ||
                (s.Service_Type != null && s.Service_Type.Contains(term)));
        }

        // Main query — left joins match the proc exactly.
        // VendorPortal is joined on Entry_Id AND pSsn == provider's dash-stripped SSN.
        var query =
            from s in baseQuery

            join m in db.Mandates.AsNoTracking()
                on s.Entry_Id equals m.Entry_Id into mGroup
            from m in mGroup.DefaultIfEmpty()

            join p in db.Providers.AsNoTracking()
                on s.Provider_Id equals p.Provider_Id into pGroup
            from p in pGroup.DefaultIfEmpty()

                // Cross-check: the assignment must belong to this provider's SSN
            join va in topAssignments
                on new
                {
                    EntryId = s.Entry_Id,
                    ProvSsn = p != null && p.Ssn != null ? p.Ssn.Replace("-", "") : null
                }
                equals new { va.EntryId, va.ProvSsn } into vaGroup
            from va in vaGroup.DefaultIfEmpty()

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

                // Alert flags
                MandateFlag = s.Entry_Id == null,
                ProviderFlag = s.Provider_Id == null,
                BRateFlag = s.bRate == null,
                PRateFlag = s.pRate == null,
                AssignFlag = va == null || va.Assign_Id == null || va.Assign_Id == "",

                // Logic flags
                IsOverDuration = s.OverDuration ?? false,
                IsOverlap = s.Overlap ?? false,
                IsOverMandate = s.OverMandate ?? false,
                IsUnderGroup = s.UnderGroup ?? false,

                // Joined fields
                ProviderLastName = s.Provider_Last_Name,
                ProviderFirstName = s.Provider_First_Name,
                AssignId = va != null ? va.Assign_Id : null,
                MandateStart = m != null ? m.MandateStart : null,

                // Masked SSN
                Ssn = (p != null && p.Ssn != null && p.Ssn.Length >= 4)
                      ? "***-**-" + p.Ssn.Substring(p.Ssn.Length - 4)
                      : (p != null ? p.Ssn : null),

                // Provider address fields from the Providers join
                // FullAddress mirrors proc: RTRIM(City)+', '+State+' '+Zipcode
                FullAddress = p != null
                    ? (p.City != null ? p.City.Trim() : "") + ", " + (p.State ?? "") + " " + (p.Zipcode ?? "")
                    : null,
            };

        // Default sort matches the proc: date_of_Service + Start_Time as datetime, then provider name.
        // ToPagedResultAsync will override this if the user clicks a sort column.
        if (string.IsNullOrWhiteSpace(request.SortBy))
        {
            request = request with { SortBy = "DateOfService", SortDir = "asc" };
        }

        // performSearch: false — search was already applied above on the raw Seses entity
        return await query.ToPagedResultAsync(request, ct, performSearch: false);
    }

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
