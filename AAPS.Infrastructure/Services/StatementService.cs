using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AAPS.Infrastructure.Services;

public class StatementService : IStatementService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private string? _headerPath;
    private string? _footerPath;

    public StatementService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void SetLogoPaths(string? headerPath, string? footerPath)
    {
        _headerPath = headerPath;
        _footerPath = footerPath;
    }

    public async Task<byte[]> GenerateAsync(string search, Dictionary<string, string> columnFilters, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        // Step 1: Resolve matching SesisIds using the same grid filters
        var request = new PagedRequest(search, columnFilters, PageSize: -1);
        var matching = await BuildBaseQuery(db).ToPagedResultAsync(request, ct, performSearch: false);

        if (matching.Items.Count == 0)
            return [];

        var matchingIds = matching.Items.Select(r => r.SesisId).ToHashSet();

        // Step 2: Fetch full row data (including Admin_DBN and provider address)
        var rows = await db.Seses.AsNoTracking()
            .Where(s => matchingIds.Contains(s.Sesis_Id))
            .Join(db.Providers,
                s => s.Provider_Id,
                p => p.Provider_Id,
                (s, p) => new StatementRow
                {
                    ProviderLast    = s.Provider_Last_Name ?? "",
                    ProviderFirst   = s.Provider_First_Name ?? "",
                    ProviderAddress = p.Address ?? "",
                    ProviderCity    = p.City ?? "",
                    ProviderState   = p.State ?? "",
                    ProviderZip     = p.Zipcode ?? "",
                    DateOfService   = s.date_of_Service,
                    StudentId       = s.Student_ID ?? "",
                    StudentLast     = s.Last_Name ?? "",
                    StudentFirst    = s.First_Name ?? "",
                    School          = s.Admin_DBN ?? "",
                    StartTime       = s.Start_Time ?? "",
                    EndTime         = s.End_Time ?? "",
                    GroupSize       = s.Actual_Size ?? "",
                    Duration        = s.Duration ?? "",
                    Frequency       = s.Assignment_Claimed ?? "",
                    ServiceType     = s.Service_Type ?? "",
                    BillingRate     = s.pRate,
                    BillingAmount   = s.pAmount,
                })
            .OrderBy(r => r.ProviderLast)
            .ThenBy(r => r.ProviderFirst)
            .ThenBy(r => r.DateOfService)
            .ThenBy(r => r.StartTime)
            .ToListAsync(ct);

        if (rows.Count == 0)
            return [];

        var providerGroups = rows
            .GroupBy(r => (r.ProviderLast, r.ProviderFirst))
            .ToList();

        return GeneratePdf(providerGroups);
    }

    // ── Same base query as BillingService (no unpaidOnly filter — statements show all filtered records) ──
    // Uses MIN(VendorPortal_Id) per (Entry_Id, pSsn) to match Acounting_Select exactly.
    private static IQueryable<BillingRecordDTO> BuildBaseQuery(AppDbContext db)
    {
        var topAssignByEntryAndSsn = db.VendorPortals.AsNoTracking()
            .Where(v => v.Entry_Id != null && v.pSsn != null)
            .GroupBy(v => new { v.Entry_Id, v.pSsn })
            .Select(g => new
            {
                EntryId = g.Key.Entry_Id,
                ProvSsn = g.Key.pSsn,
                TopId   = g.Min(x => x.VendorPortal_Id)
            });

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

    private byte[] GeneratePdf(List<IGrouping<(string Last, string First), StatementRow>> providerGroups)
    {
        var headerBytes = LoadImage(_headerPath);
        var footerBytes = LoadImage(_footerPath);
        var today = DateTime.Today.ToString("MMMM d, yyyy");

        return Document.Create(container =>
        {
            foreach (var group in providerGroups)
            {
                var rows = group.ToList();
                var first = rows[0];
                var providerName = $"{first.ProviderLast}, {first.ProviderFirst}";
                var cityStateZip = $"{first.ProviderCity}, {first.ProviderState} {first.ProviderZip}".Trim().TrimEnd(',').Trim();
                var total = rows.Sum(r => r.BillingAmount ?? 0);

                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.MarginHorizontal(36);
                    page.MarginTop(14);
                    page.MarginBottom(14);
                    page.DefaultTextStyle(x => x.FontSize(7.5f).FontFamily("Arial"));

                    // ── HEADER — repeats on every page of this provider's section ──
                    page.Header().Column(col =>
                    {
                        col.Spacing(0);

                        // Company logo
                        if (headerBytes != null)
                            col.Item().AlignCenter().Height(110).Image(headerBytes).FitHeight();
                        else
                            col.Item().AlignCenter().Height(110).AlignMiddle()
                               .Text("Related Services \"R\" Us").Bold().FontSize(14);

                        col.Item().PaddingTop(3);

                        // Provider info box (name, address | date)
                        col.Item().Border(0.5f).BorderColor(Colors.Black).Padding(4).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(providerName).Bold().FontSize(8);
                                if (!string.IsNullOrWhiteSpace(first.ProviderAddress))
                                    c.Item().Text(first.ProviderAddress).FontSize(7.5f);
                                if (!string.IsNullOrWhiteSpace(cityStateZip))
                                    c.Item().Text(cityStateZip).FontSize(7.5f);
                            });
                            row.AutoItem().AlignBottom().PaddingLeft(8).Text(today).FontSize(7.5f);
                        });

                        col.Item().PaddingTop(2);
                    });

                    // ── FOOTER — repeats on every page ──
                    page.Footer().Column(col =>
                    {
                        col.Spacing(2);
                        col.Item()
                           .Text("** Payment is made pending the acceptance of the NYC Dept. of Education")
                           .FontSize(6.5f).Italic();
                        if (footerBytes != null)
                            col.Item().AlignCenter().Height(70).Image(footerBytes).FitHeight();
                        else
                            col.Item().AlignCenter()
                               .Text("SERVICES OFFERED  ■  Occupational Therapy (OT)  ■  Speech Therapy  ■  Physical Therapy (PT)  ■  Counseling")
                               .FontSize(6.5f);
                    });

                    // ── CONTENT ──
                    page.Content().Column(contentCol =>
                    {
                        contentCol.Spacing(0);

                        contentCol.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(38);   // Date
                                c.ConstantColumn(54);   // OSIS
                                c.ConstantColumn(90);   // Student Name
                                c.ConstantColumn(34);   // School
                                c.ConstantColumn(38);   // Start Time
                                c.ConstantColumn(38);   // End Time
                                c.ConstantColumn(18);   // Grou
                                c.ConstantColumn(18);   // Dura
                                c.ConstantColumn(52);   // Freq
                                c.ConstantColumn(68);   // Service Type
                                c.ConstantColumn(40);   // Hourly Ra
                                c.RelativeColumn();     // Amount
                            });

                            t.Header(h =>
                            {
                                static void HCell(TableCellDescriptor h, string label) =>
                                    h.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1)
                                      .PaddingVertical(1).PaddingHorizontal(2)
                                      .Text(label).Italic().FontSize(7f);

                                HCell(h, "Date");
                                HCell(h, "OSIS");
                                HCell(h, "Student Name");
                                HCell(h, "School");
                                HCell(h, "Start Time");
                                HCell(h, "End Time");
                                HCell(h, "Grou");
                                HCell(h, "Dura");
                                HCell(h, "Freq");
                                HCell(h, "Service Type");
                                HCell(h, "Hourly Ra");
                                HCell(h, "Amount");
                            });

                            foreach (var r in rows)
                            {
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.DateOfService?.ToString("MM/dd/yy") ?? "").FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.StudentId).FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text($"{r.StudentLast}, {r.StudentFirst}").FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.School).FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.StartTime).FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.EndTime).FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.GroupSize).FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.Duration).FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.Frequency).FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.ServiceType).FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).Text(r.BillingRate.HasValue ? $"$ {r.BillingRate.Value:F2}" : "").FontSize(7.5f);
                                t.Cell().Border(0.3f).BorderColor(Colors.Grey.Darken1).PaddingVertical(0.5f).PaddingHorizontal(2).AlignRight().Text(r.BillingAmount?.ToString("C2") ?? "").FontSize(7.5f);
                            }
                        });

                        // Total amount — green-bordered box, centered
                        contentCol.Item().AlignCenter().PaddingTop(8)
                            .Border(1f).BorderColor("#00aa00")
                            .Padding(4)
                            .Text($"Total Amount: $ {total:N2}")
                            .Bold().FontSize(9f);
                    });
                });
            }
        }).GeneratePdf();
    }

    private static byte[]? LoadImage(string? path) =>
        !string.IsNullOrEmpty(path) && File.Exists(path) ? File.ReadAllBytes(path) : null;

    private sealed class StatementRow
    {
        public string ProviderLast    { get; set; } = "";
        public string ProviderFirst   { get; set; } = "";
        public string ProviderAddress { get; set; } = "";
        public string ProviderCity    { get; set; } = "";
        public string ProviderState   { get; set; } = "";
        public string ProviderZip     { get; set; } = "";
        public DateTime? DateOfService { get; set; }
        public string StudentId       { get; set; } = "";
        public string StudentLast     { get; set; } = "";
        public string StudentFirst    { get; set; } = "";
        public string School          { get; set; } = "";
        public string StartTime       { get; set; } = "";
        public string EndTime         { get; set; } = "";
        public string GroupSize       { get; set; } = "";
        public string Duration        { get; set; } = "";
        public string Frequency       { get; set; } = "";
        public string ServiceType     { get; set; } = "";
        public decimal? BillingRate   { get; set; }
        public decimal? BillingAmount { get; set; }
    }
}
