using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.Common.Settings;
using AAPS.Application.DTO;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace AAPS.Infrastructure.Services;

public class BillingService : IBillingService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ILogger<BillingService> _logger;
    private readonly BillingSettings _settings;

    public BillingService(IDbContextFactory<AppDbContext> factory, ILogger<BillingService> logger, IOptions<BillingSettings> settings)
    {
        _factory = factory;
        _logger = logger;
        _settings = settings.Value;
    }

    // ── Shared base query — same filters used by the grid and file generation ──
    // Matches Acounting_Select exactly: MIN(VendorPortal_Id) per (Entry_Id, pSsn),
    // then inner-join semantics via WHERE VendorPortal.Assign_Id IS NOT NULL.
    private static IQueryable<BillingRecordDTO> BuildBaseQuery(AppDbContext db, bool unpaidOnly = false)
    {
        // Step 1: MIN(VendorPortal_Id) per (Entry_Id, pSsn) — mirrors the proc subquery
        var topAssignByEntryAndSsn = db.VendorPortals.AsNoTracking()
            .Where(v => v.Entry_Id != null && v.pSsn != null)
            .GroupBy(v => new { v.Entry_Id, v.pSsn })
            .Select(g => new
            {
                EntryId = g.Key.Entry_Id,
                ProvSsn = g.Key.pSsn,
                TopId   = g.Min(x => x.VendorPortal_Id)
            });

        // Step 2: join back to get Assign_Id from that specific row
        var topAssignments =
            from t in topAssignByEntryAndSsn
            join v in db.VendorPortals.AsNoTracking() on t.TopId equals v.VendorPortal_Id
            select new { t.EntryId, t.ProvSsn, v.Assign_Id };

        var sesis = db.Seses.AsNoTracking()
            .Where(s => s.Overlap != true
                     && s.OverMandate != true
                     && s.OverDuration != true
                     && s.UnderGroup != true
                     && s.Entry_Id != null
                     && s.bRate != null
                     && s.pRate != null);

        if (unpaidOnly)
            sesis = sesis.Where(s => s.bPaid == null);

        // Step 3: join Sesis → Providers → topAssignments, filter Assign_Id IS NOT NULL
        return
            from s in sesis
            join p in db.Providers.AsNoTracking() on s.Provider_Id equals p.Provider_Id
            join va in topAssignments
                on new
                {
                    EntryId = s.Entry_Id,
                    ProvSsn = p.Ssn != null ? p.Ssn.Replace("-", "") : null
                }
                equals new { va.EntryId, va.ProvSsn }
            where va.Assign_Id != null
            select new BillingRecordDTO
            {
                SesisId        = s.Sesis_Id,
                DateOfService  = s.date_of_Service,
                StartTime      = s.Start_Time,
                EndTime        = s.End_Time,
                Provider       = s.Provider_Last_Name + ", " + s.Provider_First_Name,
                GDistrict      = s.GDistrict,
                StudentId      = s.Student_ID,
                Student        = s.Last_Name + ", " + s.First_Name,
                Grade          = s.Grade,
                ServiceType    = s.Service_Type,
                ActualSize     = s.Actual_Size,
                Duration       = s.Duration,
                Frequency      = s.Assignment_Claimed,
                Billed         = s.Billed,
                BilledPaidOn   = s.bPaid,
                ProviderPaidOn = s.pPaid,
                BillingRate    = s.bRate,
                ProviderRate   = s.pRate,
                BillingAmount  = s.bAmount,
                ProviderAmount = s.pAmount,
                AssignId       = va.Assign_Id,
                EntryId        = s.Entry_Id,
            };
    }

    public async Task<PagedResult<BillingRecordDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await BuildBaseQuery(db).ToPagedResultAsync(request, ct, performSearch: false);
    }

    public async Task<BillingSummary> GetSummaryAsync(string search, Dictionary<string, string> columnFilters, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var request = new PagedRequest(search, columnFilters);
        var result = await BuildBaseQuery(db)
            .ApplyFilters(request, performSearch: false)
            .Select(x => new { x.BillingAmount, x.ProviderAmount })
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), TotalBilling = g.Sum(x => x.BillingAmount ?? 0), TotalProvider = g.Sum(x => x.ProviderAmount ?? 0) })
            .FirstOrDefaultAsync(ct);
        return result == null ? new BillingSummary(0, 0, 0) : new BillingSummary(result.Count, result.TotalBilling, result.TotalProvider);
    }

    public async Task UpdateBillingDatesAsync(int sesisId, DateTime? billed, DateTime? billedPaidOn, DateTime? providerPaidOn, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var entity = await db.Seses.FindAsync(new object[] { sesisId }, ct)
            ?? throw new KeyNotFoundException($"Sesi record {sesisId} not found.");

        _logger.LogInformation(
            "Updating billing dates for Sesis {SesisId}: Billed={Billed}, BilledPaid={BilledPaid}, ProviderPaid={ProviderPaid}",
            sesisId, billed, billedPaidOn, providerPaidOn);

        entity.Billed = billed;
        entity.bPaid  = billedPaidOn;
        entity.pPaid  = providerPaidOn;

        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Billing dates updated for Sesis {SesisId}", sesisId);
    }

    public async Task<List<GeneratedBillingFile>> GenerateBillingFilesAsync(string search, Dictionary<string, string> columnFilters, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OutputPath))
            throw new InvalidOperationException("Billing OutputPath is not configured in appsettings.json.");

        await using var db = _factory.CreateDbContext();

        // Step 1: Resolve matching SesisIds using the same query + filters as the grid, plus bPaid IS NULL.
        // This ensures file generation always mirrors exactly what the user sees (filtered), but only unpaid records.
        var request = new PagedRequest(search, columnFilters, PageSize: -1);
        var matching = await BuildBaseQuery(db, unpaidOnly: true)
            .ToPagedResultAsync(request, ct, performSearch: false);

        if (matching.Items.Count == 0) return [];

        var matchingIds = matching.Items.Select(r => r.SesisId).ToHashSet();

        // Step 2: Fetch sesis+provider fields needed for file content.
        // Order matches the stored proc: ORDER BY date_of_Service+Start_Time, Provider_Last_Name, Provider_First_Name, Student_ID
        var sesiRows = await db.Seses.AsNoTracking()
            .Where(s => matchingIds.Contains(s.Sesis_Id))
            .Join(db.Providers,
                s => s.Provider_Id,
                p => p.Provider_Id,
                (s, p) => new
                {
                    s.Sesis_Id,
                    s.Entry_Id,
                    s.date_of_Service,
                    s.GDistrict,
                    s.Student_ID,
                    s.Last_Name,
                    s.First_Name,
                    s.Service_Type,
                    s.Actual_Size,
                    s.Language_Provided,
                    s.Start_Time,
                    s.End_Time,
                    s.Duration,
                    s.bAmount,
                    s.Provider_Last_Name,
                    s.Provider_First_Name,
                    SsnStripped = p.Ssn!.Substring(0, 3) + p.Ssn.Substring(4, 2) + p.Ssn.Substring(7, 4),
                })
            .OrderBy(r => r.date_of_Service)
            .ThenBy(r => r.Start_Time)
            .ThenBy(r => r.Provider_Last_Name)
            .ThenBy(r => r.Provider_First_Name)
            .ThenBy(r => r.Student_ID)
            .ToListAsync(ct);

        var entryIds = sesiRows.Select(r => r.Entry_Id!.Value).Distinct().ToList();

        // Step 3: Fetch mandates — Remaining_Freq and Grp_Size for aSes/aFreq/aGrs/aType/aYear
        var mandates = await db.Mandates.AsNoTracking()
            .Where(m => entryIds.Contains(m.Entry_Id))
            .Select(m => new { m.Entry_Id, m.MandateStart, m.MandateEnd, m.Remaining_Freq, m.Grp_Size })
            .ToListAsync(ct);
        var mandateDict = mandates.ToDictionary(m => m.Entry_Id);

        // Step 4: Fetch vendor portal — MIN(VendorPortal_Id) per (Entry_Id, pSsn), matches proc subquery
        var vendorPortals = await db.VendorPortals.AsNoTracking()
            .Where(v => v.Entry_Id.HasValue && entryIds.Contains(v.Entry_Id.Value) && v.Assign_Id != null)
            .Select(v => new { v.VendorPortal_Id, v.Entry_Id, v.pSsn, v.pFund, v.pBoro, v.pSchool, v.Assign_Id })
            .ToListAsync(ct);
        var vpDict = vendorPortals
            .GroupBy(v => (v.Entry_Id, v.pSsn))
            .ToDictionary(g => g.Key, g => g.OrderBy(v => v.VendorPortal_Id).First());

        // Step 5: Build file rows, group by (FundCode, BillingMonth)
        var groups = sesiRows
            .Select(r =>
            {
                vpDict.TryGetValue((r.Entry_Id, r.SsnStripped), out var vp);
                if (vp == null) return null;

                var fundCode = vp.pFund ?? "";
                if (fundCode is not ("4410" or "4411" or "4412")) return null;

                mandateDict.TryGetValue(r.Entry_Id!.Value, out var m);
                var dos = r.date_of_Service;

                // aYear: CASE WHEN MONTH(MandateStart) > 6 THEN YEAR(MandateStart)+1 ELSE YEAR(MandateStart) END
                var aYear = m?.MandateStart.HasValue == true
                    ? (m.MandateStart.Value.Month > 6
                        ? m.MandateStart.Value.Year + 1
                        : m.MandateStart.Value.Year).ToString()
                    : "";

                // aType: LEFT(Service_Type,1) + group size logic
                var svcFirst = r.Service_Type?.Length > 0 ? r.Service_Type[0].ToString() : "";
                var grpInt = int.TryParse(m?.Grp_Size?.Trim(), out var gs) ? gs : 0;
                var aType = svcFirst + (grpInt == 1 ? "1" : svcFirst == "S" ? "P" : "T");

                // aSes: '0' + LEFT(Remaining_Freq, 1)
                var remFreq = m?.Remaining_Freq ?? "";
                var aSes = "0" + (remFreq.Length > 0 ? remFreq[0].ToString() : "");

                // aFreq: SUBSTRING(Remaining_Freq, 4, 1) — SQL is 1-indexed so char at C# index 3
                var aFreq = remFreq.Length >= 4 ? remFreq[3].ToString() : "";

                // aGrs: '0' + Grp_Size
                var aGrs = "0" + (m?.Grp_Size ?? "");

                // Date fields formatted as MM/dd/yyyy (SQL CONVERT format 101).
                // MandateStart, MandateEnd, bServiceDate use CONVERT(char(12),...) which pads to 12 chars
                // — MM/dd/yyyy is 10 chars so 2 trailing spaces are appended to match WinForms output exactly.
                var bServiceDate = (dos?.ToString("MM/dd/yyyy") ?? "") + "  ";
                var invoiceMonth = dos.HasValue
                    ? dos.Value.ToString("MM") + "/01/" + dos.Value.Year
                    : "";
                var mandateStart = (m?.MandateStart?.ToString("MM/dd/yyyy") ?? "") + "  ";
                var mandateEnd   = (m?.MandateEnd?.ToString("MM/dd/yyyy") ?? "") + "  ";

                var lang = r.Language_Provided ?? "";
                var langCode = lang.Length >= 2 ? lang.Substring(0, 2).ToUpper() : lang.ToUpper();

                return new
                {
                    FundCode     = fundCode,
                    BillingMonth = dos?.ToString("yyyy-MM") ?? "",
                    Row = BuildRow(
                        aYear, vp.pBoro ?? "", r.GDistrict ?? "", fundCode, vp.pSchool ?? "",
                        r.Provider_Last_Name ?? "", r.Provider_First_Name ?? "", r.SsnStripped,
                        r.Student_ID ?? "", r.First_Name ?? "", r.Last_Name ?? "",
                        aType, mandateStart, mandateEnd,
                        aSes, aFreq, r.Duration ?? "", aGrs,
                        langCode, vp.Assign_Id ?? "", invoiceMonth, bServiceDate,
                        r.Actual_Size ?? "", r.Start_Time ?? "", r.End_Time ?? "", r.bAmount)
                };
            })
            .Where(x => x != null)
            .GroupBy(x => (x!.FundCode, x.BillingMonth))
            .ToList();

        // Step 6: Write files — OutputPath/{year}/{month}/{fundCode}_{yyyy-MM}_{ddMMyyHHmm}.txt
        var now = DateTime.Now;
        var destFolder = Path.Combine(_settings.OutputPath, now.Year.ToString(), now.Month.ToString("D2"));
        Directory.CreateDirectory(destFolder);
        var timestamp = now.ToString("ddMMyyHHmm");

        const string Header =
            "SRAP_FISCAL_YR\tSRAP_BORO_CD\tSRAP_DIST_CD\tSRAP_FUND_CD\tSRAP_SCHL_ID\tSRAP_PROVIDER_TYPE\tSRAP_AGENCY_CD\tSRAP_PROVIDER\tPROVIDER_LAST_NAME\tPROVIDER_FIRST_NAME\t" +
            "SRAP_ACT_PROVIDER\tSRAP_OSIS_ID\tSTUD_FIRST_NAME\tSTUD_LAST_NAME\tSRAP_SERV_SUBTYPE\tSRAP_START_DT\tSRAP_END_DT\tSRAP_SESSIONS\tSRAP_FREQ_TERM\tSRAP_SESS_LEN\tSRAP_GROUP_SIZE\t" +
            "SRAP_LANG_CD\tSRAP_ASSIGN_ID\tSCIN_INVOICE_MONTH\tSCIN_INVOICE_DAYS\tSCIN_ATTEND_CODE\tSCIN_ACT_GRP_SIZE\tSCIN_START_TIME\tSCIN_END_TIME\tSCIN_SCHOOL_OTHER\t/\tSCIN_VEND_INVOICE\tSCIN_INVOICE_AMT\tCOMPENSATORY_RECOVERY_SERVICE_FLAG\t/\t/\t/\t/\t/\t";

        var result = new List<GeneratedBillingFile>();

        foreach (var group in groups)
        {
            var (fundCode, billingMonth) = group.Key;
            var fileName = $"{fundCode}_{billingMonth}_{timestamp}.txt";
            var filePath = Path.Combine(destFolder, fileName);

            var sb = new StringBuilder();
            sb.AppendLine(Header);
            foreach (var item in group)
                sb.AppendLine(item!.Row);

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            await File.WriteAllBytesAsync(filePath, bytes, ct);

            _logger.LogInformation("Generated billing file {FileName} with {Count} records", fileName, group.Count());
            result.Add(new GeneratedBillingFile(fileName, filePath, bytes));
        }

        return result;
    }

    private static string BuildRow(
        string aYear, string boro, string district, string fund, string school,
        string providerLast, string providerFirst, string ssn,
        string studentId, string studentFirst, string studentLast,
        string aType, string mandateStart, string mandateEnd,
        string aSes, string aFreq, string aDur, string aGrs,
        string lang, string assignId, string invoiceMonth, string bServiceDate,
        string actualSize, string startTime, string endTime, decimal? amount)
    {
        return new StringBuilder()
            .Append(aYear).Append('\t')
            .Append(boro).Append('\t')
            .Append(district).Append('\t')
            .Append(fund).Append('\t')
            .Append(school).Append('\t')
            .Append("A\t")
            .Append("AD08\t")
            .Append("463312411\t")
            .Append(providerLast).Append('\t')
            .Append(providerFirst).Append('\t')
            .Append(ssn).Append('\t')
            .Append(studentId).Append('\t')
            .Append(studentFirst).Append('\t')
            .Append(studentLast).Append('\t')
            .Append(aType).Append('\t')
            .Append(mandateStart).Append('\t')
            .Append(mandateEnd).Append('\t')
            .Append(aSes).Append('\t')
            .Append(aFreq).Append('\t')
            .Append(aDur).Append('\t')
            .Append(aGrs).Append('\t')
            .Append(lang).Append('\t')
            .Append(assignId).Append('\t')
            .Append(invoiceMonth).Append('\t')
            .Append(bServiceDate).Append('\t')
            .Append("P\t")
            .Append(actualSize).Append('\t')
            .Append(startTime).Append('\t')
            .Append(endTime).Append('\t')
            .Append("S\t")
            .Append('\t')           // / (empty)
            .Append('\t')           // SCIN_VEND_INVOICE (empty)
            .Append(amount?.ToString("F2")).Append('\t')
            .Append("\t\t\t\t\t\t") // COMPENSATORY_RECOVERY_SERVICE_FLAG + 5 trailing /
            .ToString();
    }
}
