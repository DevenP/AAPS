using AAPS.Application.Abstractions.Data;
using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Settings;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AAPS.Infrastructure.Services;

public class ImportService : IImportService
{
    private readonly IAppDbContext _db;
    private readonly IImportLogService _importLogService;
    private readonly ImportSettings _settings;
    private readonly ILogger<ImportService> _logger;

    public ImportService(IAppDbContext db, IImportLogService importLogService, IOptions<ImportSettings> settings, ILogger<ImportService> logger)
    {
        _db = db;
        _importLogService = importLogService;
        _settings = settings.Value;
        _logger = logger;
    }


    // ─────────────────────────────────────────────
    // PARSE
    // ─────────────────────────────────────────────

    // Expected headers per import type: col index -> substring to match (case-insensitive)
    // A handful of distinctive columns per file type to fingerprint the upload
    private static readonly Dictionary<ImportType, Dictionary<int, string>> _expectedHeaders = new()
    {
        [ImportType.Mandates] = new()
        {
            [5] = "Student ID",
            [6] = "Last Name",
            [7] = "First Name",
            [19] = "Service Type",
            [39] = "Mandate ID",
        },
        [ImportType.Sesis] = new()
        {
            [1] = "Student ID",
            [25] = "Date of Service",
            [30] = "Session Type",
            [36] = "Duration",
            [41] = "Provider Last Name",
        },
        [ImportType.VendorPortal] = new()
        {
            [12] = "OSIS",
            [18] = "Sessions",
            [20] = "Sess Len",
            [21] = "Group Size",
            [23] = "Assign",
        },
        [ImportType.Payments] = new()
        {
            [1] = "VOUCH",
            [7] = "OSIS",
            [9] = "PROVIDER",
            [15] = "SESS",
            [16] = "START",
        },
    };

    public async Task<ImportPreviewResult> ParseAsync(ImportType type, string fileName, Stream fileStream)
    {
        // 1. Extension check
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Invalid file type. Only .xlsx files are supported. Received: \"{Path.GetExtension(fileName)}\"");

        // 2. Read into memory
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        // 3. Size check
        if (fileBytes.Length > _settings.MaxFileSizeBytes)
            throw new InvalidOperationException(
                $"File is too large ({fileBytes.Length / 1024 / 1024}MB). Maximum allowed size is {_settings.MaxFileSizeBytes / 1024 / 1024}MB.");

        // 4. Try opening as workbook
        ms.Position = 0;
        XLWorkbook workbook;
        try { workbook = new XLWorkbook(ms); }
        catch
        {
            throw new InvalidOperationException(
                "The file could not be opened as an Excel workbook. It may be corrupted or not a valid .xlsx file.");
        }

        using (workbook)
        {
            IXLWorksheet ws;
            try { ws = workbook.Worksheet(1); }
            catch { throw new InvalidOperationException("The workbook has no worksheets."); }

            // 5. Empty file check
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
            int dataStartRow = type == ImportType.Mandates ? 4 : 2;
            if (lastRow < dataStartRow)
                throw new InvalidOperationException(
                    "The file appears to be empty — no data rows were found.");

            // 6. Header fingerprint — hard block if wrong file
            int headerRow = type == ImportType.Mandates ? 3 : 1;
            var expected = _expectedHeaders[type];
            var mismatches = new List<string>();

            foreach (var (col, expectedText) in expected)
            {
                var actual = ws.Cell(headerRow, col).GetValue<string>()?.Trim() ?? "";
                if (!actual.Contains(expectedText, StringComparison.OrdinalIgnoreCase))
                    mismatches.Add(
                        $"Col {col}: expected \"{expectedText}\", " +
                        $"found \"{(actual.Length > 30 ? actual[..30] + "…" : actual)}\"");
            }

            if (mismatches.Any())
            {
                string typeName = type switch
                {
                    ImportType.Mandates => "Approvals",
                    ImportType.Sesis => "Provider Billing",
                    ImportType.VendorPortal => "Vendor Portal",
                    ImportType.Payments => "Voucher Payments",
                    _ => type.ToString()
                };
                throw new InvalidOperationException(
                    $"This file does not look like a valid {typeName} file. " +
                    $"Header mismatch(es):\n{string.Join("\n", mismatches)}");
            }

            return type switch
            {
                ImportType.Mandates => ParseMandates(ws, fileName, fileBytes),
                ImportType.Sesis => ParseSesis(ws, fileName, fileBytes),
                ImportType.VendorPortal => ParseVendorPortal(ws, fileName, fileBytes),
                ImportType.Payments => ParsePayments(ws, fileName, fileBytes),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }


    // ─────────────────────────────────────────────
    // MANDATES PARSE
    // Data starts row 4, headers on row 3
    // Skip if any required col is null: 5,6,7,12,19,21,23,25,27,29,37,39
    // Row display number = i - 3
    // ─────────────────────────────────────────────
    private static ImportPreviewResult ParseMandates(IXLWorksheet ws, string fileName, byte[] fileBytes)
    {
        var valid = new List<ImportRowResult>();
        var skipped = new List<ImportRowResult>();

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

        for (int i = 4; i <= lastRow; i++)
        {
            int displayRow = i - 3;

            string? Get(int col) => ws.Cell(i, col).IsEmpty() ? null : ws.Cell(i, col).GetValue<string>()?.Trim();
            DateTime? GetDate(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetDateTime(); } catch { return null; }
            }

            // Required columns
            var required = new[] { 5, 6, 7, 12, 19, 21, 23, 25, 27, 29, 37, 39 };
            bool anyNull = required.Any(col => ws.Cell(i, col).IsEmpty());

            var preview = new Dictionary<string, string?>
            {
                ["Row #"] = displayRow.ToString(),
                ["Student ID"] = Get(5),
                ["Last Name"] = Get(6),
                ["First Name"] = Get(7),
                ["DOB"] = GetDate(12)?.ToString("MM/dd/yyyy"),
                ["Service Type"] = Get(19),
                ["Language"] = Get(21),
                ["Grp Size"] = Get(23),
                ["Duration"] = Get(25),
                ["Remaining Freq"] = Get(27),
                ["Provider"] = Get(29),
                ["First Attend Date"] = GetDate(37)?.ToString("MM/dd/yyyy"),
                ["Mandate ID"] = Get(39),
            };

            // Collect all skip reasons for this row
            var reasons = new List<string>();

            if (anyNull)
                reasons.Add("Missing required field(s)");

            // DOB must parse as a real date
            if (!ws.Cell(i, 12).IsEmpty() && GetDate(12) == null)
                reasons.Add("Date of Birth is not a valid date");

            // First Attend Date must parse as a real date
            if (!ws.Cell(i, 37).IsEmpty() && GetDate(37) == null)
                reasons.Add("First Attend Date is not a valid date");

            // Grp Size must be numeric
            if (!ws.Cell(i, 23).IsEmpty() && !int.TryParse(Get(23), out _))
                reasons.Add($"Grp Size \"{Get(23)}\" is not a number");

            // Student ID max 25 chars
            var studentId = Get(5);
            if (studentId?.Length > 25)
                reasons.Add($"Student ID exceeds 25 characters ({studentId.Length})");

            if (reasons.Any())
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = string.Join("; ", reasons), PreviewColumns = preview });
                continue;
            }

            valid.Add(new ImportRowResult { RowNumber = i, IsValid = true, PreviewColumns = preview });
        }

        return new ImportPreviewResult { ValidRows = valid, SkippedRows = skipped, FileName = fileName, FileBytes = fileBytes };
    }

    // ─────────────────────────────────────────────
    // SESIS PARSE
    // Data starts row 2, headers on row 1
    // Skip if required cols null: 1,2,3,5,9,13,15,19,21,24,25,28,30,31,32,34,35,36,41,42
    // Also skip if col 30 doesn't contain "SERVICE PROVIDED" (case-insensitive)
    // Row display number = i - 1
    // ─────────────────────────────────────────────
    private static ImportPreviewResult ParseSesis(IXLWorksheet ws, string fileName, byte[] fileBytes)
    {
        var valid = new List<ImportRowResult>();
        var skipped = new List<ImportRowResult>();

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

        for (int i = 2; i <= lastRow; i++)
        {
            int displayRow = i - 1;

            string? Get(int col) => ws.Cell(i, col).IsEmpty() ? null : ws.Cell(i, col).GetValue<string>()?.Trim();
            DateTime? GetDate(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetDateTime(); } catch { return null; }
            }

            var required = new[] { 1, 2, 3, 5, 9, 13, 15, 19, 21, 24, 25, 28, 30, 31, 32, 34, 35, 36, 41, 42 };
            bool anyNull = required.Any(col => ws.Cell(i, col).IsEmpty());

            string? sessionType = Get(30);
            bool sessionTypeInvalid = string.IsNullOrWhiteSpace(sessionType) ||
                                      !sessionType.ToUpper().Contains("SERVICE PROVIDED");

            var preview = new Dictionary<string, string?>
            {
                ["Row #"] = displayRow.ToString(),
                ["Student ID"] = Get(1),
                ["Last Name"] = Get(2),
                ["First Name"] = Get(3),
                ["DOB"] = GetDate(5)?.ToString("MM/dd/yyyy"),
                ["Admin DBN"] = Get(9),
                ["Service Type"] = Get(26),
                ["Language"] = Get(28),
                ["Session Type"] = sessionType,
                ["Date of Service"] = GetDate(25)?.ToString("MM/dd/yyyy"),
                ["Duration"] = Get(36),
                ["Provider Last"] = Get(41),
                ["Provider First"] = Get(42),
            };

            var reasons = new List<string>();

            if (anyNull)
                reasons.Add("Missing required field(s)");

            if (sessionTypeInvalid)
                reasons.Add("Session type does not contain 'Service Provided'");

            // Date of Birth must parse
            if (!ws.Cell(i, 5).IsEmpty() && GetDate(5) == null)
                reasons.Add("Date of Birth is not a valid date");

            // Date of Service must parse
            if (!ws.Cell(i, 25).IsEmpty() && GetDate(25) == null)
                reasons.Add("Date of Service is not a valid date");

            // Duration must be numeric
            var dur = Get(36);
            if (!string.IsNullOrWhiteSpace(dur) && !int.TryParse(dur, out _))
                reasons.Add($"Duration \"{dur}\" is not a number");

            // Start/End time must contain AM or PM
            var startTime = Get(34);
            var endTime = Get(35);
            if (!string.IsNullOrWhiteSpace(startTime) && !startTime.ToUpper().Contains("M"))
                reasons.Add($"Start Time \"{startTime}\" is not in AM/PM format");
            if (!string.IsNullOrWhiteSpace(endTime) && !endTime.ToUpper().Contains("M"))
                reasons.Add($"End Time \"{endTime}\" is not in AM/PM format");

            // Student ID max 25 chars
            var sid = Get(1);
            if (sid?.Length > 25)
                reasons.Add($"Student ID exceeds 25 characters ({sid.Length})");

            if (reasons.Any())
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = string.Join("; ", reasons), PreviewColumns = preview });
                continue;
            }

            valid.Add(new ImportRowResult { RowNumber = i, IsValid = true, PreviewColumns = preview });
        }

        return new ImportPreviewResult { ValidRows = valid, SkippedRows = skipped, FileName = fileName, FileBytes = fileBytes };
    }

    // ─────────────────────────────────────────────
    // VENDOR PORTAL PARSE
    // Data starts row 2, headers on row 1
    // Skip if col 23 (Assign_Id) is null
    // Deduplicate by Assign_Id using a HashSet
    // ─────────────────────────────────────────────
    private static ImportPreviewResult ParseVendorPortal(IXLWorksheet ws, string fileName, byte[] fileBytes)
    {
        var valid = new List<ImportRowResult>();
        var skipped = new List<ImportRowResult>();
        var seenAssignIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

        for (int i = 2; i <= lastRow; i++)
        {
            int displayRow = i - 1;

            string? Get(int col) => ws.Cell(i, col).IsEmpty() ? null : ws.Cell(i, col).GetValue<string>()?.Trim();
            DateTime? GetDate(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetDateTime(); } catch { return null; }
            }

            string? assignId = Get(23);

            var preview = new Dictionary<string, string?>
            {
                ["Row #"] = displayRow.ToString(),
                ["Assign ID"] = assignId,
                ["Student ID"] = Get(12),
                ["Boro"] = Get(2),
                ["District"] = Get(3),
                ["Duration"] = Get(20),
                ["Freq"] = $"{Get(18)}x {Get(19)}",
                ["Grp Size"] = Get(21),
                ["Start Date"] = GetDate(16)?.ToString("MM/dd/yyyy"),
                ["SSN (col11)"] = Get(11),
            };

            if (string.IsNullOrWhiteSpace(assignId))
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = "Missing Assign ID", PreviewColumns = preview });
                continue;
            }

            if (!seenAssignIds.Add(assignId))
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = $"Duplicate Assign ID: {assignId}", PreviewColumns = preview });
                continue;
            }

            // Duration must be numeric
            var vpDur = Get(20);
            if (!string.IsNullOrWhiteSpace(vpDur) && !int.TryParse(vpDur, out _))
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = $"Duration \"{vpDur}\" is not a number", PreviewColumns = preview });
                continue;
            }

            // Student ID max 25 chars
            var vpSid = Get(12);
            if (vpSid?.Length > 25)
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = $"Student ID exceeds 25 characters ({vpSid.Length})", PreviewColumns = preview });
                continue;
            }

            valid.Add(new ImportRowResult { RowNumber = i, IsValid = true, PreviewColumns = preview });
        }

        return new ImportPreviewResult { ValidRows = valid, SkippedRows = skipped, FileName = fileName, FileBytes = fileBytes };
    }

    // ─────────────────────────────────────────────
    // PAYMENTS PARSE
    // Data starts row 2, headers on row 1
    // Skip if col 1 (Voucher) is null
    // This is an UPDATE operation, not insert
    // ─────────────────────────────────────────────
    private static ImportPreviewResult ParsePayments(IXLWorksheet ws, string fileName, byte[] fileBytes)
    {
        var valid = new List<ImportRowResult>();
        var skipped = new List<ImportRowResult>();

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

        for (int i = 2; i <= lastRow; i++)
        {
            int displayRow = i - 1;

            string? Get(int col) => ws.Cell(i, col).IsEmpty() ? null : ws.Cell(i, col).GetValue<string>()?.Trim();
            DateTime? GetDate(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetDateTime(); } catch { return null; }
            }
            string? GetTime(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try
                {
                    // Try as DateTime first (Excel time serial)
                    var dt = cell.GetDateTime();
                    return dt.ToString("h:mm tt");
                }
                catch
                {
                    // Fallback: parse raw string "12:30:00" -> "12:30 PM"
                    var raw = cell.GetValue<string>()?.Trim();
                    if (raw == null) return null;
                    if (TimeSpan.TryParse(raw, out var ts))
                        return DateTime.Today.Add(ts).ToString("h:mm tt");
                    return raw;
                }
            }

            string? voucher = Get(1);

            var preview = new Dictionary<string, string?>
            {
                ["Row #"] = displayRow.ToString(),
                ["Voucher"] = voucher,
                ["Student ID"] = Get(7),
                ["SSN"] = Get(9),
                ["Provider"] = Get(10),
                ["Date of Service"] = GetDate(15)?.ToString("MM/dd/yyyy"),
                ["Start Time"] = GetTime(16),
            };

            if (string.IsNullOrWhiteSpace(voucher))
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = "Missing Voucher number", PreviewColumns = preview });
                continue;
            }

            // Date of Service must parse
            if (!ws.Cell(i, 15).IsEmpty() && GetDate(15) == null)
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = "Date of Service is not a valid date", PreviewColumns = preview });
                continue;
            }

            // Student ID max 25 chars
            var pSid = Get(7);
            if (pSid?.Length > 25)
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = $"Student ID exceeds 25 characters ({pSid.Length})", PreviewColumns = preview });
                continue;
            }

            valid.Add(new ImportRowResult { RowNumber = i, IsValid = true, PreviewColumns = preview });
        }

        return new ImportPreviewResult { ValidRows = valid, SkippedRows = skipped, FileName = fileName, FileBytes = fileBytes };
    }

    // ─────────────────────────────────────────────
    // COMMIT
    // ─────────────────────────────────────────────

    public async Task<ImportCommitResult> CommitAsync(ImportType type, ImportPreviewResult preview, CancellationToken ct = default)
    {
        var result = type switch
        {
            ImportType.Mandates => await CommitMandatesAsync(preview, ct),
            ImportType.Sesis => await CommitSesisAsync(preview, ct),
            ImportType.VendorPortal => await CommitVendorPortalAsync(preview, ct),
            ImportType.Payments => await CommitPaymentsAsync(preview, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        // Write ImportLog
        string prefix = type switch
        {
            ImportType.Mandates => "[Approvals]",
            ImportType.Sesis => "[Provider Billing]",
            ImportType.VendorPortal => "[Vendor Portal]",
            ImportType.Payments => "[Voucher Payments]",
            _ => "[Unknown]"
        };

        string importRecord;
        if (type == ImportType.Payments)
        {
            importRecord = $"Complete Import. {result.Updated} of {result.Updated + result.Skipped} rows matched and updated.";
        }
        else if (result.SkippedRowNumbers.Any())
        {
            importRecord = "Skipped Records: " + string.Join("; ", result.SkippedRowNumbers) + ";";
        }
        else
        {
            importRecord = "Complete Import.";
        }

        await _importLogService.CreateAsync(new ImportLogDTO
        {
            FileName = $"{prefix} {preview.FileName}",
            ImportRecord = importRecord,
            ImportDate = DateTime.Now
        }, ct);

        return result;
    }

    // ─────────────────────────────────────────────
    // COMMIT MANDATES
    // ─────────────────────────────────────────────
    private async Task<ImportCommitResult> CommitMandatesAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        using var workbook = new XLWorkbook(new MemoryStream(preview.FileBytes));
        var ws = workbook.Worksheet(1);

        int inserted = 0;
        var skippedRowNumbers = new List<int>();

        // Add already-skipped rows from parse phase
        skippedRowNumbers.AddRange(preview.SkippedRows.Select(r => r.RowNumber));

        foreach (var row in preview.ValidRows)
        {
            int i = row.RowNumber; // actual Excel row index
            try
            {
                string? Get(int col) => ws.Cell(i, col).IsEmpty() ? null : ws.Cell(i, col).GetValue<string>()?.Trim();
                DateTime? GetDate(int col)
                {
                    var cell = ws.Cell(i, col);
                    if (cell.IsEmpty()) return null;
                    try { return cell.GetDateTime(); } catch { return null; }
                }

                string studentId = Get(5)!;
                string serviceType = Get(19)!;
                string remainingFreq = Get(27)!;
                string dur = Get(25)!;
                string grpSize = Get(23)!;
                string mandateId = Get(39)!;

                // Duplicate check
                bool exists = await _db.Mandates.AnyAsync(m =>
                    m.Student_ID == studentId &&
                    m.Service_Type == serviceType &&
                    m.Remaining_Freq == remainingFreq &&
                    m.Dur == dur &&
                    m.Grp_Size == grpSize &&
                    m.Mandate_ID == mandateId, ct);

                if (exists)
                {
                    skippedRowNumbers.Add(i - 3);
                    continue;
                }

                // Calculate MandateStart / MandateEnd
                DateTime? firstAttendDate = GetDate(37) ?? DateTime.Now;
                DateTime mandateStart = firstAttendDate.Value.Date;
                DateTime mandateEnd;
                int month = mandateStart.Month;
                if (month == 7)
                    mandateEnd = new DateTime(mandateStart.Year, 8, 31, 23, 59, 0);
                else if (month > 7)
                    mandateEnd = new DateTime(mandateStart.Year + 1, 6, 30, 23, 59, 0);
                else
                    mandateEnd = new DateTime(mandateStart.Year, 6, 30, 23, 59, 0);

                var entity = new Mandate
                {
                    Conf_Date = GetDate(3),
                    Student_ID = studentId,
                    Last_Name = Get(6),
                    First_Name = Get(7),
                    Home_District = Get(8)?.ToString(),
                    CSE = Get(9),
                    CSE_District = Get(10)?.ToString(),
                    Grade = Get(11),
                    Date_of_Birth = GetDate(12),
                    Admin_DBN = Get(13),
                    D75 = Get(17),
                    Service_Type = serviceType,
                    Lang = Get(21),
                    Grp_Size = grpSize,
                    Dur = dur,
                    Service_Location = Get(26),
                    Remaining_Freq = remainingFreq,
                    Provider = Get(29),
                    Service_Start_Date = GetDate(35),
                    First_Attend_Date = firstAttendDate,
                    Mandate_ID = mandateId,
                    Primary_Contact_Phone_1 = Get(42),
                    Primary_Contact_Phone_2 = Get(43),
                    MandateStart = mandateStart,
                    MandateEnd = mandateEnd,
                    FileName = preview.FileName,
                    RowNumber = i
                };

                _db.Mandates.Add(entity);
                await _db.SaveChangesAsync(ct);

                int newEntryId = entity.Entry_Id;

                // Backfill unlinked Sesis rows that match this mandate
                if (int.TryParse(dur.Split(' ')[0], out int durInt))
                {
                    int grpSizeInt = int.TryParse(grpSize, out var g) ? g : 0;

                    var matchingSesis = await _db.Seses
                        .Where(s => s.Entry_Id == null &&
                                    s.Service_Type!.Trim() == serviceType.Trim() &&
                                    s.Student_ID!.Trim() == studentId.Trim() &&
                                    s.date_of_Service >= mandateStart &&
                                    s.date_of_Service <= mandateEnd)
                        .ToListAsync(ct);

                    foreach (var sesi in matchingSesis)
                    {
                        if (!int.TryParse(sesi.Duration, out int sesiDur)) continue;
                        if (sesiDur != durInt) continue;

                        int actualSize = int.TryParse(sesi.Actual_Size?.TrimStart('0'), out var a) ? a : 1;
                        bool groupMatch = (grpSizeInt == 1 && actualSize == 1) ||
                                         (grpSizeInt > 1 && grpSizeInt >= actualSize);
                        if (!groupMatch) continue;

                        sesi.Entry_Id = newEntryId;
                    }

                    await _db.SaveChangesAsync(ct);
                }

                inserted++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CommitMandatesAsync: skipping row {Row} due to error", i - 3);
                skippedRowNumbers.Add(i - 3);
            }
        }

        return new ImportCommitResult
        {
            Inserted = inserted,
            Skipped = skippedRowNumbers.Count,
            SkippedRowNumbers = skippedRowNumbers
        };
    }

    // ─────────────────────────────────────────────
    // COMMIT SESIS
    // Bulk lookups upfront, then batch insert every 500 rows
    // ─────────────────────────────────────────────
    private async Task<ImportCommitResult> CommitSesisAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        using var workbook = new XLWorkbook(new MemoryStream(preview.FileBytes));
        var ws = workbook.Worksheet(1);

        int inserted = 0;
        var skippedRowNumbers = new List<int>();
        skippedRowNumbers.AddRange(preview.SkippedRows.Select(r => r.RowNumber));

        // ── Bulk lookups ──────────────────────────────────────────────────

        // 1. Existing duplicate keys: StudentId|ServiceType|DOSDate|StartTime|EndTime|ProviderLast|ProviderFirst|ActualSize
        var existingKeys = await _db.Seses
            .Where(s => s.date_of_Service.HasValue)
            .Select(s => s.Student_ID + "|" + s.Service_Type + "|" +
                         s.date_of_Service!.Value.Date.ToString("yyyyMMdd") + "|" +
                         s.Start_Time + "|" + s.End_Time + "|" +
                         s.Provider_Last_Name + "|" + s.Provider_First_Name + "|" + s.Actual_Size)
            .ToHashSetAsync(ct);

        // 2. All providers: "LastName,FirstName" -> Provider_Id
        var providerDict = await _db.Providers
            .Select(p => new { Key = (p.LastName ?? "").Trim() + "," + (p.FirstName ?? "").Trim(), p.Provider_Id })
            .ToDictionaryAsync(p => p.Key, p => (int?)p.Provider_Id, StringComparer.OrdinalIgnoreCase, ct);

        // 3. All mandates for in-memory matching
        var allMandates = await _db.Mandates
            .Select(m => new
            {
                m.Entry_Id,
                m.Student_ID,
                m.Service_Type,
                m.Dur,
                m.Grp_Size,
                m.MandateStart,
                m.MandateEnd
            })
            .ToListAsync(ct);

        // 4. All active billing rates: "ServiceType|District|Lang" -> Rate
        var billingRateDict = await _db.BillingRates
            .Where(b => b.Active == true)
            .Select(b => new { Key = (b.ServiceType ?? "").Trim() + "|" + (b.District ?? "").Trim() + "|" + (b.Lang ?? "").Trim(), b.Rate })
            .ToDictionaryAsync(b => b.Key, b => (decimal?)b.Rate, StringComparer.OrdinalIgnoreCase, ct);

        // 5. All active provider rates: "ServiceType|District|Lang|ProviderId" -> Rate
        var providerRateDict = await _db.ProviderRates
            .Where(p => p.Active == true)
            .Select(p => new { Key = (p.ServiceType ?? "").Trim() + "|" + (p.District ?? "").Trim() + "|" + (p.Lang ?? "").Trim() + "|" + p.Provider_Id, p.Rate })
            .ToDictionaryAsync(p => p.Key, p => (decimal?)p.Rate, StringComparer.OrdinalIgnoreCase, ct);

        // ── Row processing ────────────────────────────────────────────────

        int batchSize = _settings.BatchSize;
        var batch = new List<Sesi>(batchSize);

        async Task FlushBatchAsync()
        {
            if (batch.Count == 0) return;
            try
            {
                _db.Seses.AddRange(batch);
                await _db.SaveChangesAsync(ct);
                inserted += batch.Count;
            }
            catch (Exception ex)
            {
                // Batch failed — fall back to row-by-row so we can skip only the bad ones
                _logger.LogWarning(ex, "CommitSesisAsync: batch of {Count} rows failed, falling back to row-by-row", batch.Count);
                ((DbContext)_db).ChangeTracker.Clear();
                foreach (var e in batch)
                {
                    try
                    {
                        _db.Seses.Add(e);
                        await _db.SaveChangesAsync(ct);
                        inserted++;
                    }
                    catch (Exception rowEx)
                    {
                        _logger.LogWarning(rowEx, "CommitSesisAsync: skipping row {Row} due to error", e.RowNumber);
                        if (e.RowNumber.HasValue)
                            skippedRowNumbers.Add(e.RowNumber.Value - 1);
                    }
                }
            }
            batch.Clear();
        }

        foreach (var row in preview.ValidRows)
        {
            int i = row.RowNumber;

            string? Get(int col) => ws.Cell(i, col).IsEmpty() ? null : ws.Cell(i, col).GetValue<string>()?.Trim();
            DateTime? GetDate(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetDateTime(); } catch { return null; }
            }

            string startTime = Get(34) ?? "";
            string endTime = Get(35) ?? "";

            // Zero-pad Actual_Size
            string rawActualSize = Get(33) ?? "";
            string actualSize = string.IsNullOrWhiteSpace(rawActualSize) ? "01" : ("0" + rawActualSize.Trim());

            string studentId = Get(1)!;
            string serviceType = Get(26) ?? "";
            DateTime? dateOfService = GetDate(25);
            string providerLast = Get(41)!;
            string providerFirst = Get(42)!;
            string duration = Get(36) ?? "";
            string gDistrict = Get(13) ?? "";
            string language = Get(28) ?? "";

            // Duplicate check via HashSet
            string dupKey = studentId + "|" + serviceType + "|" +
                            (dateOfService?.Date.ToString("yyyyMMdd") ?? "") + "|" +
                            startTime + "|" + endTime + "|" +
                            providerLast + "|" + providerFirst + "|" + actualSize;
            if (existingKeys.Contains(dupKey))
            {
                skippedRowNumbers.Add(i - 1);
                continue;
            }

            // Provider_Id lookup
            string provKey = providerLast + "," + providerFirst;
            providerDict.TryGetValue(provKey, out int? providerId);

            // Entry_Id lookup via in-memory mandates
            int? entryId = null;
            if (int.TryParse(duration, out int durInt) && dateOfService.HasValue)
            {
                int actualSizeInt = int.TryParse(actualSize.TrimStart('0'), out var a) ? a : 1;
                if (actualSizeInt == 0) actualSizeInt = 1;

                entryId = allMandates
                    .Where(m =>
                        string.Equals(m.Service_Type?.Trim(), serviceType.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(m.Student_ID?.Trim(), studentId.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        m.MandateStart <= dateOfService &&
                        m.MandateEnd >= dateOfService)
                    .Where(m =>
                    {
                        if (!int.TryParse(m.Dur?.Split(' ').FirstOrDefault(), out int mDur)) return false;
                        if (mDur != durInt) return false;
                        int mGrp = int.TryParse(m.Grp_Size, out var g) ? g : 0;
                        return (mGrp == 1 && actualSizeInt == 1) || (mGrp > 1 && mGrp >= actualSizeInt);
                    })
                    .Select(m => (int?)m.Entry_Id)
                    .FirstOrDefault();
            }

            // Billing rate lookup
            string rateKey = serviceType.Trim() + "|" + gDistrict.Trim() + "|" + language.Trim();
            billingRateDict.TryGetValue(rateKey, out decimal? bRate);

            // Provider rate lookup
            decimal? pRate = null;
            if (providerId.HasValue)
            {
                string pRateKey = serviceType.Trim() + "|" + gDistrict.Trim() + "|" + language.Trim() + "|" + providerId.Value;
                providerRateDict.TryGetValue(pRateKey, out pRate);
            }

            // Calculate amounts
            int actualSizeForCalc = int.TryParse(actualSize.TrimStart('0'), out var ac) ? ac : 1;
            if (actualSizeForCalc == 0) actualSizeForCalc = 1;
            decimal? bAmount = (bRate.HasValue && durInt > 0) ? bRate.Value * durInt / 60.0m / actualSizeForCalc : null;
            decimal? pAmount = (pRate.HasValue && durInt > 0) ? pRate.Value * durInt / 60.0m / actualSizeForCalc : null;

            // Mandatetime_Start: col 16, only if length > 5
            string? mandateStartRaw = Get(16);
            DateTime? mandateTimeStart = null;
            if (!string.IsNullOrWhiteSpace(mandateStartRaw) && mandateStartRaw.Length > 5)
            {
                try { mandateTimeStart = ws.Cell(i, 16).GetDateTime(); } catch { }
            }

            batch.Add(new Sesi
            {
                Student_ID = studentId,
                Last_Name = Get(2),
                First_Name = Get(3),
                Grade = Get(4),
                date_of_Birth = GetDate(5),
                Home_District = Get(6),
                CSE = Get(7),
                CSE_District = Get(8),
                Admin_DBN = Get(9),
                GDistrict = gDistrict,
                Borough = Get(14),
                Mandate_Short = Get(15),
                Mandatetime_Start = mandateTimeStart,
                Mandated_Max_Group = Get(19),
                Assignment_First_Encounter = GetDate(21),
                Assignment_Claimed = Get(24),
                date_of_Service = dateOfService,
                Service_Type = serviceType,
                Language_Provided = language,
                Session_Type = Get(30),
                Session_Notes = Get(31),
                Groupin = Get(32),
                Actual_Size = actualSize,
                Start_Time = startTime,
                End_Time = endTime,
                Duration = duration,
                Provider_Last_Name = providerLast,
                Provider_First_Name = providerFirst,
                FileName = preview.FileName,
                RowNumber = i,
                Provider_Id = providerId,
                Entry_Id = entryId,
                bRate = bRate,
                pRate = pRate,
                bAmount = bAmount,
                pAmount = pAmount,
                Overlap = false,
                OverMandate = false,
                OverDuration = false
            });

            // Also add to local dup set so rows within this same import don't duplicate each other
            existingKeys.Add(dupKey);

            if (batch.Count >= batchSize)
                await FlushBatchAsync();
        }

        await FlushBatchAsync();

        return new ImportCommitResult
        {
            Inserted = inserted,
            Skipped = skippedRowNumbers.Count,
            SkippedRowNumbers = skippedRowNumbers
        };
    }

    // ─────────────────────────────────────────────
    // COMMIT VENDOR PORTAL
    // Bulk dup check upfront, batch insert every 500, then backfill Entry_Id
    // ─────────────────────────────────────────────
    private async Task<ImportCommitResult> CommitVendorPortalAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        using var workbook = new XLWorkbook(new MemoryStream(preview.FileBytes));
        var ws = workbook.Worksheet(1);

        int inserted = 0;
        var skippedRowNumbers = new List<int>();
        skippedRowNumbers.AddRange(preview.SkippedRows.Select(r => r.RowNumber));

        // ── Bulk lookups ──────────────────────────────────────────────────

        // 1. All existing Assign_Ids
        var existingAssignIds = await _db.VendorPortals
            .Where(v => v.Assign_Id != null)
            .Select(v => v.Assign_Id!)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

        // 2. All providers: SSN (dashes stripped) -> Provider entity
        var providersBySsn = await _db.Providers
            .Where(p => p.Ssn != null)
            .ToListAsync(ct);

        // 3. All mandates for Entry_Id backfill matching
        var allMandates = await _db.Mandates
            .Select(m => new
            {
                m.Entry_Id,
                m.Student_ID,
                m.Dur,
                m.Remaining_Freq,
                m.Grp_Size,
                m.MandateStart
            })
            .ToListAsync(ct);

        // ── Row processing ────────────────────────────────────────────────

        // Build a lookup of AssignId -> display row number for skip tracking in fallback
        var assignIdToDisplayRow = preview.ValidRows
            .ToDictionary(
                r => ws.Cell(r.RowNumber, 23).GetValue<string>()?.Trim() ?? "",
                r => r.RowNumber - 1,
                StringComparer.OrdinalIgnoreCase);

        int batchSize = _settings.BatchSize;
        var batch = new List<VendorPortal>(batchSize);
        var batchAssignIds = new List<string>(); // parallel list for skip tracking

        async Task FlushBatchAsync()
        {
            if (batch.Count == 0) return;
            try
            {
                _db.VendorPortals.AddRange(batch);
                await _db.SaveChangesAsync(ct);
                inserted += batch.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CommitVendorPortalAsync: batch of {Count} rows failed, falling back to row-by-row", batch.Count);
                ((DbContext)_db).ChangeTracker.Clear();
                for (int b = 0; b < batch.Count; b++)
                {
                    var e = batch[b];
                    try
                    {
                        _db.VendorPortals.Add(e);
                        await _db.SaveChangesAsync(ct);
                        inserted++;
                    }
                    catch (Exception rowEx)
                    {
                        _logger.LogWarning(rowEx, "CommitVendorPortalAsync: skipping assign {AssignId} due to error", batchAssignIds[b]);
                        if (assignIdToDisplayRow.TryGetValue(batchAssignIds[b], out int dispRow))
                            skippedRowNumbers.Add(dispRow);
                    }
                }
            }
            batch.Clear();
            batchAssignIds.Clear();
        }

        foreach (var row in preview.ValidRows)
        {
            int i = row.RowNumber;

            string? Get(int col) => ws.Cell(i, col).IsEmpty() ? null : ws.Cell(i, col).GetValue<string>()?.Trim();
            DateTime? GetDate(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetDateTime(); } catch { return null; }
            }

            string assignId = Get(23)!;

            // Duplicate check via HashSet
            if (existingAssignIds.Contains(assignId))
            {
                skippedRowNumbers.Add(i - 1);
                continue;
            }

            existingAssignIds.Add(assignId); // prevent duplicates within this import

            string pFreq = $"{Get(18)}x {Get(19)}";

            batch.Add(new VendorPortal
            {
                pSsn = Get(11),
                pBoro = Get(2),
                pDist = Get(3),
                pFund = Get(4),
                pSchool = Get(5),
                Student_ID = Get(12),
                pFreq = pFreq,
                pDur = Get(20),
                pGrpSize = Get(21),
                pStartDate = GetDate(16),
                Assign_Id = assignId,
                VPFile = preview.FileName
            });
            batchAssignIds.Add(assignId);

            if (batch.Count >= batchSize)
                await FlushBatchAsync();
        }

        await FlushBatchAsync();

        // ── Entry_Id backfill pass ────────────────────────────────────────
        // Now that rows are inserted and have VendorPortal_Ids, link Entry_Id
        // Only process rows that were successfully inserted (no Entry_Id yet)
        var newRows = await _db.VendorPortals
            .Where(v => v.Entry_Id == null && v.VPFile == preview.FileName)
            .ToListAsync(ct);

        foreach (var vp in newRows)
        {
            if (string.IsNullOrWhiteSpace(vp.pSsn)) continue;

            string pSsn = vp.pSsn;
            string pFreq4 = (vp.pFreq?.Length >= 4) ? vp.pFreq[..4] : (vp.pFreq ?? "");

            var provider = providersBySsn
                .FirstOrDefault(p => p.Ssn?.Replace("-", "") == pSsn);
            if (provider == null) continue;

            var matchedMandate = allMandates.FirstOrDefault(m =>
            {
                if (!string.Equals(m.Student_ID?.Trim(), (vp.Student_ID ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) return false;
                if (!int.TryParse(vp.pDur, out int vpDurInt)) return false;
                if (!int.TryParse(m.Dur?.Split(' ').FirstOrDefault(), out int mDurInt)) return false;
                if (vpDurInt != mDurInt) return false;
                string mFreq4 = (m.Remaining_Freq?.Length >= 4) ? m.Remaining_Freq[..4] : (m.Remaining_Freq ?? "");
                if (mFreq4 != pFreq4) return false;
                if (!int.TryParse(vp.pGrpSize, out int vpGrp)) return false;
                if (!int.TryParse(m.Grp_Size, out int mGrp)) return false;
                if (vpGrp != mGrp) return false;
                if (!vp.pStartDate.HasValue || !m.MandateStart.HasValue) return false;
                return vp.pStartDate.Value.Date == m.MandateStart.Value.Date;
            });

            if (matchedMandate != null)
                vp.Entry_Id = matchedMandate.Entry_Id;
        }

        await _db.SaveChangesAsync(ct);

        return new ImportCommitResult
        {
            Inserted = inserted,
            Skipped = skippedRowNumbers.Count,
            SkippedRowNumbers = skippedRowNumbers
        };
    }

    // ─────────────────────────────────────────────
    // COMMIT PAYMENTS
    // Updates Sesis rows: sets bPaid = now, Voucher = voucher number
    // Matches on student, date of service, start time, provider SSN last 4, provider name
    // ─────────────────────────────────────────────
    private async Task<ImportCommitResult> CommitPaymentsAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        using var workbook = new XLWorkbook(new MemoryStream(preview.FileBytes));
        var ws = workbook.Worksheet(1);

        int updated = 0;
        int noMatch = 0;
        var skippedRowNumbers = new List<int>();
        skippedRowNumbers.AddRange(preview.SkippedRows.Select(r => r.RowNumber));

        // ── Bulk lookups upfront (2 queries total instead of 2 per row) ──────
        // Collect the date range and student IDs from the file first
        var rowData = new List<(int RowNumber, string Voucher, string StudentId, string? SsnLast4, string? Provider, DateTime DosDate, string StartTimeNormalized)>();

        foreach (var row in preview.ValidRows)
        {
            int i = row.RowNumber;
            string? Get(int col) => ws.Cell(i, col).IsEmpty() ? null : ws.Cell(i, col).GetValue<string>()?.Trim();
            DateTime? GetDate(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetDateTime(); } catch { return null; }
            }

            string voucher = Get(1)!;
            string? studentId = Get(7);
            string? ssn = Get(9);
            string? provider = Get(10);
            DateTime? dateOfService = GetDate(15);

            // Normalize start time from col 16 (e.g. "12:30:00" -> "12:30 PM")
            string? startTimeRaw = Get(16);
            string? startTimeNormalized = null;
            if (!string.IsNullOrWhiteSpace(startTimeRaw))
            {
                startTimeNormalized = TimeSpan.TryParse(startTimeRaw, out var ts)
                    ? DateTime.Today.Add(ts).ToString("h:mm tt")
                    : startTimeRaw;
            }

            if (string.IsNullOrWhiteSpace(studentId) || !dateOfService.HasValue || string.IsNullOrWhiteSpace(startTimeNormalized))
            {
                noMatch++;
                continue;
            }

            string? ssnLast4 = ssn?.Length >= 4 ? ssn.Substring(ssn.Length - 4) : ssn;
            rowData.Add((i, voucher, studentId, ssnLast4, provider, dateOfService.Value.Date, startTimeNormalized));
        }

        if (rowData.Count == 0)
            return new ImportCommitResult { Updated = 0, Skipped = noMatch, SkippedRowNumbers = skippedRowNumbers };

        // Load all relevant Sesis in one query (all students + date range in this file)
        var studentIds = rowData.Select(r => r.StudentId).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var minDate = rowData.Min(r => r.DosDate);
        var maxDate = rowData.Max(r => r.DosDate);

        var allCandidateSesis = await _db.Seses
            .Where(s => s.Student_ID != null &&
                        studentIds.Contains(s.Student_ID.Trim()) &&
                        s.date_of_Service.HasValue &&
                        s.date_of_Service.Value.Date >= minDate &&
                        s.date_of_Service.Value.Date <= maxDate)
            .ToListAsync(ct);

        // Load all providers referenced by those Sesis in one query
        var providerIds = allCandidateSesis.Select(s => s.Provider_Id).Where(id => id.HasValue).Distinct().ToList();
        var allProviders = await _db.Providers
            .Where(p => providerIds.Contains(p.Provider_Id))
            .ToListAsync(ct);
        var providerById = allProviders.ToDictionary(p => p.Provider_Id);

        // ── In-memory matching loop ────────────────────────────────────────
        foreach (var r in rowData)
        {
            var candidates = allCandidateSesis
                .Where(s => string.Equals(s.Student_ID?.Trim(), r.StudentId, StringComparison.OrdinalIgnoreCase) &&
                            s.date_of_Service.HasValue &&
                            s.date_of_Service.Value.Date == r.DosDate)
                .ToList();

            int matchCount = 0;
            foreach (var sesi in candidates)
            {
                // Normalize stored Start_Time: strip leading zero "02:45 PM" -> "2:45 PM"
                string? storedTime = sesi.Start_Time;
                if (!string.IsNullOrWhiteSpace(storedTime) && storedTime.StartsWith("0"))
                    storedTime = storedTime.Substring(1);

                if (!string.Equals(storedTime?.Trim(), r.StartTimeNormalized, StringComparison.OrdinalIgnoreCase)) continue;

                if (!sesi.Provider_Id.HasValue || !providerById.TryGetValue(sesi.Provider_Id.Value, out var prov)) continue;
                if (r.SsnLast4 != null && (prov.Ssn == null || !prov.Ssn.EndsWith(r.SsnLast4))) continue;
                if (r.Provider != null &&
                    !r.Provider.Contains(prov.LastName ?? "", StringComparison.OrdinalIgnoreCase) &&
                    !r.Provider.Contains(prov.FirstName ?? "", StringComparison.OrdinalIgnoreCase)) continue;

                sesi.bPaid = DateTime.Now;
                sesi.Voucher = r.Voucher;
                matchCount++;
            }

            if (matchCount > 0)
                updated += matchCount;
            else
                noMatch++;
        }

        // ── Single save for all changes ────────────────────────────────────
        if (updated > 0)
        {
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CommitPaymentsAsync: failed to save {Count} payment updates", updated);
                throw;
            }
        }

        return new ImportCommitResult
        {
            Updated = updated,
            Skipped = noMatch,
            SkippedRowNumbers = skippedRowNumbers
        };
    }

    // ─────────────────────────────────────────────
    // ARCHIVE
    // ─────────────────────────────────────────────
    public async Task ArchiveFileAsync(ImportType type, string fileName, byte[] fileBytes)
    {
        string basePath = type switch
        {
            ImportType.Mandates => _settings.MandatesArchivePath,
            ImportType.Sesis => _settings.SesisArchivePath,
            ImportType.VendorPortal => _settings.VendorPortalArchivePath,
            ImportType.Payments => _settings.PaymentsArchivePath,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        if (string.IsNullOrWhiteSpace(basePath)) return;

        Directory.CreateDirectory(basePath);
        var destPath = Path.Combine(basePath, fileName);

        // Avoid overwriting — append timestamp if file exists
        if (File.Exists(destPath))
        {
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            destPath = Path.Combine(basePath, $"{name}_{ts}{ext}");
        }

        await File.WriteAllBytesAsync(destPath, fileBytes);
    }
}
