using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Services;

public class SesiService : ISesiService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ILogger<SesiService> _logger;

    public SesiService(IDbContextFactory<AppDbContext> factory, ILogger<SesiService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

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
            VoucherAmount = dto.VoucherAmount,
            VoucherBalancePaid = dto.VoucherBalancePaid,
            OverMandate = dto.IsOverMandate,
            OverDuration = dto.IsOverDuration,
            UnderGroup = dto.IsUnderGroup
        };
        _logger.LogInformation("Creating sesi record for student {StudentId} on {DateOfService}",
            dto.StudentId, dto.DateOfService?.ToString("yyyy-MM-dd"));

        db.Seses.Add(entity);
        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Sesi {Id} created for student {StudentId}", entity.Sesis_Id, dto.StudentId);

        return entity.Sesis_Id;
    }

    public async Task UpdateAsync(int id, SesiDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Seses.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException();

        _logger.LogInformation("Updating sesi {Id} for student {StudentId}", id, entity.Student_ID);

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
        entity.VoucherAmount = dto.VoucherAmount;
        entity.VoucherBalancePaid = dto.VoucherBalancePaid;
        entity.OverMandate = dto.IsOverMandate;
        entity.OverDuration = dto.IsOverDuration;
        entity.UnderGroup = dto.IsUnderGroup;

        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Sesi {Id} updated", id);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Seses.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _logger.LogInformation("Deleting sesi {Id} for student {StudentId}", id, entity.Student_ID);
            db.Seses.Remove(entity);
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Sesi {Id} deleted", id);
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
                MandateEnd   = m != null ? m.MandateEnd   : null,

                // Masked SSN
                Ssn = (p != null && p.Ssn != null && p.Ssn.Length >= 4)
                      ? "***-**-" + p.Ssn.Substring(p.Ssn.Length - 4)
                      : (p != null ? p.Ssn : null),

                // Provider address fields from the Providers join
                // FullAddress mirrors proc: RTRIM(City)+', '+State+' '+Zipcode
                FullAddress = p != null
                    ? (p.City != null ? p.City.Trim() : "") + ", " + (p.State ?? "") + " " + (p.Zipcode ?? "")
                    : null,

                VoucherBalancePaid = s.VoucherBalancePaid,
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

    public async Task BulkUpdateAsync(List<int> ids, OperationEditDTO dto, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entities = await db.Seses.Where(s => ids.Contains(s.Sesis_Id)).ToListAsync(ct);
        _logger.LogInformation("BulkUpdate: {Count} sesi records", entities.Count);

        // Pre-fetch lookup data needed for rate recalculation
        bool ratesNeeded    = dto.ApplyAll || dto.ProviderId.HasValue || dto.GDistrict != null || dto.LanguageProvided != null;
        bool bRatesNeeded   = dto.ApplyAll || dto.GDistrict != null || dto.LanguageProvided != null;
        bool providerNeeded = dto.ProviderId.HasValue;

        var providerRates = ratesNeeded
            ? await db.ProviderRates.AsNoTracking().Where(r => r.Active == true).ToListAsync(ct)
            : [];

        var billingRates = bRatesNeeded
            ? await db.BillingRates.AsNoTracking().Where(r => r.Active == true).ToListAsync(ct)
            : [];

        Provider? newProvider = providerNeeded
            ? await db.Providers.AsNoTracking().FirstOrDefaultAsync(p => p.Provider_Id == dto.ProviderId, ct)
            : null;

        foreach (var e in entities)
        {
            // CSE='2' — date only, no recalc
            if (dto.ApplyAll || dto.DateOfService.HasValue)
                e.date_of_Service = dto.DateOfService;

            // CSE='3' — normalize time, recalc Duration + amounts
            if (dto.ApplyAll || dto.StartTime != null)
            {
                e.Start_Time = NormalizeTime(dto.StartTime);
                var dur = CalcDurationMinutes(e.date_of_Service, e.Start_Time, e.End_Time);
                if (dur.HasValue) { e.Duration = dur.Value.ToString(); RecalcAmounts(e, dur.Value); }
            }

            // CSE='4' — normalize time, recalc Duration + amounts
            if (dto.ApplyAll || dto.EndTime != null)
            {
                e.End_Time = NormalizeTime(dto.EndTime);
                var dur = CalcDurationMinutes(e.date_of_Service, e.Start_Time, e.End_Time);
                if (dur.HasValue) { e.Duration = dur.Value.ToString(); RecalcAmounts(e, dur.Value); }
            }

            // CSE='5' — recalc amounts using existing Duration
            if (dto.ApplyAll || dto.ActualSize != null)
            {
                e.Actual_Size = dto.ActualSize;
                if (int.TryParse(e.Duration, out var dur) && dur > 0)
                    RecalcAmounts(e, dur);
            }

            // CSE='6' — denormalize name, lookup pRate, recalc pAmount
            if (dto.ApplyAll || dto.ProviderId.HasValue)
            {
                e.Provider_Id = dto.ProviderId;
                if (dto.ProviderId.HasValue)
                {
                    e.Provider_Last_Name  = newProvider?.LastName;
                    e.Provider_First_Name = newProvider?.FirstName;
                    var pr = providerRates.FirstOrDefault(r =>
                        r.Provider_Id == dto.ProviderId &&
                        r.ServiceType == e.Service_Type &&
                        r.District    == e.GDistrict &&
                        r.Lang        == e.Language_Provided);
                    e.pRate = pr?.Rate;
                    if (e.pRate.HasValue && int.TryParse(e.Duration, out var dur) && int.TryParse(e.Actual_Size, out var sz) && sz > 0)
                        e.pAmount = e.pRate.Value * dur / 60.0m / sz;
                }
                else
                {
                    // ApplyAll + null = clear provider billing
                    e.Provider_Last_Name  = null;
                    e.Provider_First_Name = null;
                    e.pRate   = null;
                    e.pAmount = null;
                }
            }

            // CSE='10' — lookup pRate + bRate, recalc all amounts
            if (dto.ApplyAll || dto.GDistrict != null)
            {
                e.GDistrict = dto.GDistrict;
                var pr = providerRates.FirstOrDefault(r =>
                    r.Provider_Id == e.Provider_Id &&
                    r.ServiceType == e.Service_Type &&
                    r.District    == dto.GDistrict &&
                    r.Lang        == e.Language_Provided);
                var br = billingRates.FirstOrDefault(r =>
                    r.ServiceType == e.Service_Type &&
                    r.District    == dto.GDistrict &&
                    r.Lang        == e.Language_Provided);
                e.pRate = pr?.Rate;
                e.bRate = br?.Rate;
                if (int.TryParse(e.Duration, out var dur) && int.TryParse(e.Actual_Size, out var sz) && sz > 0)
                {
                    if (e.bRate.HasValue) e.bAmount = e.bRate.Value * dur / 60.0m / sz;
                    if (e.pRate.HasValue) e.pAmount = e.pRate.Value * dur / 60.0m / sz;
                }
            }

            // CSE='12' — lookup pRate + bRate, recalc all amounts
            if (dto.ApplyAll || dto.LanguageProvided != null)
            {
                e.Language_Provided = dto.LanguageProvided;
                var pr = providerRates.FirstOrDefault(r =>
                    r.Provider_Id == e.Provider_Id &&
                    r.ServiceType == e.Service_Type &&
                    r.District    == e.GDistrict &&
                    r.Lang        == dto.LanguageProvided);
                var br = billingRates.FirstOrDefault(r =>
                    r.ServiceType == e.Service_Type &&
                    r.District    == e.GDistrict &&
                    r.Lang        == dto.LanguageProvided);
                e.pRate = pr?.Rate;
                e.bRate = br?.Rate;
                if (int.TryParse(e.Duration, out var dur) && int.TryParse(e.Actual_Size, out var sz) && sz > 0)
                {
                    if (e.bRate.HasValue) e.bAmount = e.bRate.Value * dur / 60.0m / sz;
                    if (e.pRate.HasValue) e.pAmount = e.pRate.Value * dur / 60.0m / sz;
                }
            }

            // CSE='9' — link Entry_Id (simplified; proc validates student/group match)
            if (dto.ApplyAll || dto.EntryId.HasValue)
                e.Entry_Id = dto.EntryId;
        }

        // CSE='11' — update Mandates table for MandateStart (also sets First_Attend_Date)
        // CSE='14' — update Mandates table for MandateEnd (normalized to 23:59)
        if (dto.ApplyAll || dto.MandateStart.HasValue || dto.MandateEnd.HasValue)
        {
            var entryIds = entities
                .Where(e => e.Entry_Id.HasValue)
                .Select(e => e.Entry_Id!.Value)
                .Distinct()
                .ToList();

            if (entryIds.Count > 0)
            {
                var mandates = await db.Mandates.Where(m => entryIds.Contains(m.Entry_Id)).ToListAsync(ct);
                foreach (var m in mandates)
                {
                    if (dto.ApplyAll || dto.MandateStart.HasValue)
                    {
                        m.MandateStart      = dto.MandateStart;
                        m.First_Attend_Date = dto.MandateStart;
                    }
                    if (dto.ApplyAll || dto.MandateEnd.HasValue)
                    {
                        // Proc normalizes: CONVERT(char(10),@MandateEnd,1) + ' 23:59'
                        m.MandateEnd = dto.MandateEnd.HasValue
                            ? dto.MandateEnd.Value.Date.AddHours(23).AddMinutes(59)
                            : null;
                    }
                }
            }
        }

        await db.SaveChangesAsync(ct);
        await db.Database.ExecuteSqlRawAsync("EXEC OverLapMandate", ct);
        _logger.LogInformation("BulkUpdate complete");
    }

    // CSE='15' — set VoucherBalancePaid = now, only if currently null
    public async Task BulkMarkVoucherBalancePaidAsync(List<int> ids, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entities = await db.Seses
            .Where(s => ids.Contains(s.Sesis_Id) && s.VoucherBalancePaid == null)
            .ToListAsync(ct);
        _logger.LogInformation("BulkMarkVoucherBalancePaid: {Count} sesi records", entities.Count);
        var now = DateTime.Now;
        foreach (var e in entities)
            e.VoucherBalancePaid = now;
        await db.SaveChangesAsync(ct);
    }

    // CSE='16' — clear VoucherBalancePaid, only where VoucherAmount <> bAmount and it is currently set
    public async Task BulkUnmarkVoucherBalancePaidAsync(List<int> ids, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entities = await db.Seses
            .Where(s => ids.Contains(s.Sesis_Id) &&
                        s.VoucherBalancePaid != null &&
                        s.VoucherAmount != s.bAmount)
            .ToListAsync(ct);
        _logger.LogInformation("BulkUnmarkVoucherBalancePaid: {Count} sesi records", entities.Count);
        foreach (var e in entities)
            e.VoucherBalancePaid = null;
        await db.SaveChangesAsync(ct);
    }

    // Pads single-digit hour: "9:30 AM" → "09:30 AM" (mirrors proc: IF SUBSTRING(@Time,2,1)=':')
    private static string? NormalizeTime(string? time) =>
        time != null && time.Length >= 2 && time[1] == ':' ? "0" + time : time;

    // Minutes between start and end on the date of service
    private static int? CalcDurationMinutes(DateTime? dos, string? startTime, string? endTime)
    {
        if (!dos.HasValue || string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
            return null;
        var date = dos.Value.ToString("MM/dd/yyyy");
        return DateTime.TryParse($"{date} {startTime}", out var s) &&
               DateTime.TryParse($"{date} {endTime}",   out var e)
            ? (int)(e - s).TotalMinutes
            : null;
    }

    // Rate * Duration(min) / 60 / GroupSize — matches proc formula for bAmount/pAmount
    private static void RecalcAmounts(Sesi e, int durationMinutes)
    {
        if (!int.TryParse(e.Actual_Size, out var size) || size <= 0) return;
        if (e.bRate.HasValue) e.bAmount = e.bRate.Value * durationMinutes / 60.0m / size;
        if (e.pRate.HasValue) e.pAmount = e.pRate.Value * durationMinutes / 60.0m / size;
    }

    public async Task BulkUnlinkApprovalIdAsync(List<int> ids, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entities = await db.Seses.Where(s => ids.Contains(s.Sesis_Id)).ToListAsync(ct);
        _logger.LogInformation("BulkUnlinkApprovalId: {Count} sesi records", entities.Count);

        foreach (var e in entities)
            e.Entry_Id = null;

        await db.SaveChangesAsync(ct);
        await db.Database.ExecuteSqlRawAsync("EXEC OverLapMandate", ct);
        _logger.LogInformation("BulkUnlinkApprovalId complete");
    }

    public async Task<int> BulkDeleteProviderBillingAsync(List<int> ids, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        // Only delete rows with no approval linked (Entry_Id IS NULL) — mirrors WinForms behaviour
        var entities = await db.Seses
            .Where(s => ids.Contains(s.Sesis_Id) && s.Entry_Id == null)
            .ToListAsync(ct);
        _logger.LogInformation("BulkDeleteProviderBilling: {Count} records eligible (Entry_Id IS NULL)", entities.Count);

        db.Seses.RemoveRange(entities);
        await db.SaveChangesAsync(ct);
        await db.Database.ExecuteSqlRawAsync("EXEC OverLapMandate", ct);
        _logger.LogInformation("BulkDeleteProviderBilling complete");
        return entities.Count;
    }

    // Mirrors Mandates_By_Osis:
    // Returns all mandates for a student with STRING_AGG of "Provider: AssignId" pairs per mandate.
    // The STRING_AGG is replicated in-memory after two small bulk queries.
    public async Task<List<OsisMandateDTO>> GetOsisMandatesAsync(string studentId, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var mandates = await db.Mandates.AsNoTracking()
            .Where(m => m.Student_ID == studentId)
            .OrderBy(m => m.Entry_Id)
            .ToListAsync(ct);

        if (mandates.Count == 0)
            return [];

        // Fetch all VendorPortal rows with a non-null pSsn and Assign_Id
        var vpRaw = await db.VendorPortals.AsNoTracking()
            .Where(v => v.Entry_Id != null && v.pSsn != null && v.Assign_Id != null)
            .Select(v => new { v.Entry_Id, v.pSsn, AssignId = v.Assign_Id!.Trim() })
            .ToListAsync(ct);

        // Fetch all providers with an SSN (strip dashes to match pSsn format)
        var provRaw = await db.Providers.AsNoTracking()
            .Where(p => p.Ssn != null)
            .Select(p => new { p.LastName, p.FirstName, SsnStripped = p.Ssn!.Replace("-", "") })
            .ToListAsync(ct);

        var provBySsn = provRaw
            .GroupBy(p => p.SsnStripped)
            .ToDictionary(g => g.Key, g => g.First());

        // Group by Entry_Id and build the aggregated string (mirrors STRING_AGG)
        var vpByEntry = vpRaw
            .Where(v => provBySsn.ContainsKey(v.pSsn!))
            .GroupBy(v => v.Entry_Id!.Value)
            .ToDictionary(g => g.Key, g =>
                string.Join("; ", g.Select(v =>
                {
                    var p = provBySsn[v.pSsn!];
                    return $"{p.LastName}, {p.FirstName}: {v.AssignId}";
                })));

        return mandates.Select(m => new OsisMandateDTO
        {
            EntryId          = m.Entry_Id,
            ServiceType      = m.Service_Type,
            AdminDbn         = m.Admin_DBN,
            Language         = m.Lang,
            RemainingFrequency = m.Remaining_Freq,
            Duration         = m.Dur,
            GroupSize        = m.Grp_Size,
            MandateStart     = m.MandateStart,
            MandateEnd       = m.MandateEnd,
            AssignIds        = vpByEntry.TryGetValue(m.Entry_Id, out var agg) ? agg : null
        }).ToList();
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
        VoucherAmount = s.VoucherAmount,
        VoucherBalancePaid = s.VoucherBalancePaid,
        IsOverMandate = s.OverMandate ?? false,
        IsOverDuration = s.OverDuration ?? false,
        IsUnderGroup = s.UnderGroup ?? false
    };

}
