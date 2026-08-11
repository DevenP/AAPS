using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Settings;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Data.Scaffolded;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AAPS.Infrastructure.Services;

public class ImportService : IImportService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IImportLogService _importLogService;
    private readonly ImportSettings _settings;
    private readonly ILogger<ImportService> _logger;

    public ImportService(IDbContextFactory<AppDbContext> factory, IImportLogService importLogService, IOptions<ImportSettings> settings, ILogger<ImportService> logger)
    {
        _factory = factory;
        _importLogService = importLogService;
        _settings = settings.Value;
        _logger = logger;
    }


    // PARSE

    // Expected headers per import type: col index -> substring to match (case-insensitive)
    // A handful of distinctive columns per file type to fingerprint the upload
    private static readonly Dictionary<ImportType, Dictionary<int, string>> _expectedHeaders = new()
    {
        [ImportType.Mandates] = new()
        {
            [6] = "Student ID",
            [7] = "Last Name",
            [8] = "First Name",
            [21] = "Service Type",
            [43] = "Mandate ID",
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
        [ImportType.EvalPayments] = new()
        {
            [1] = "FISCAL",
            [4] = "OSIS",
            [8] = "SUBTYPE",
            [16] = "AMOUNT",
            [17] = "VOUCHER",
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

            // 6. Header fingerprint - hard block if wrong file
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
                    ImportType.EvalPayments => "Evaluation Voucher Payments",
                    _ => type.ToString()
                };
                throw new InvalidOperationException(
                    $"This file does not look like a valid {typeName} file. " +
                    $"Header mismatch(es):\n{string.Join("\n", mismatches)}");
            }

            _logger.LogInformation("Parsing {Type} file: {FileName} ({Bytes} bytes)", type, fileName, fileBytes.Length);

            var parseResult = type switch
            {
                ImportType.Mandates => ParseMandates(ws, fileName, fileBytes),
                ImportType.Sesis => ParseSesis(ws, fileName, fileBytes),
                ImportType.VendorPortal => ParseVendorPortal(ws, fileName, fileBytes),
                ImportType.Payments => ParsePayments(ws, fileName, fileBytes),
                ImportType.EvalPayments => ParseEvalPayments(ws, fileName, fileBytes),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            _logger.LogInformation("Parse complete for {FileName}: {Valid} valid row(s), {Skipped} skipped",
                fileName, parseResult.ValidRows.Count, parseResult.SkippedRows.Count);

            return parseResult;
        }
    }


    // MANDATES PARSE
    // Data starts row 4, headers on row 3
    // Skip if any required col is null: 6,7,8,13,21,23,25,27,29,32,41,43
    // Row display number = i - 3
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
                if (cell.Value.IsText)
                {
                    if (DateTime.TryParse(cell.GetValue<string>(), out var dt)) return dt;
                    return null;
                }
                try { return cell.GetDateTime(); } catch { return null; }
            }

            // Required columns
            var required = new[] { 6, 7, 8, 13, 21, 23, 25, 27, 29, 32, 41, 43 };
            bool anyNull = required.Any(col => ws.Cell(i, col).IsEmpty());

            var preview = new Dictionary<string, string?>
            {
                ["Row #"] = displayRow.ToString(),
                ["Student ID"] = Get(6),
                ["Last Name"] = Get(7),
                ["First Name"] = Get(8),
                ["DOB"] = GetDate(13)?.ToString("MM/dd/yyyy"),
                ["Service Type"] = Get(21),
                ["Language"] = Get(23),
                ["Grp Size"] = Get(25),
                ["Duration"] = Get(27),
                ["Remaining Freq"] = Get(29),
                ["Provider"] = Get(32),
                ["First Attend Date"] = GetDate(41)?.ToString("MM/dd/yyyy"),
                ["Mandate ID"] = Get(43),
            };

            // Collect all skip reasons for this row
            var reasons = new List<string>();

            if (anyNull)
                reasons.Add("Missing required field(s)");

            // DOB must parse as a real date
            if (!ws.Cell(i, 13).IsEmpty() && GetDate(13) == null)
                reasons.Add("Date of Birth is not a valid date");

            // First Attend Date must parse as a real date
            if (!ws.Cell(i, 41).IsEmpty() && GetDate(41) == null)
                reasons.Add("First Attend Date is not a valid date");

            // Grp Size must be numeric
            if (!ws.Cell(i, 25).IsEmpty() && !int.TryParse(Get(25), out _))
                reasons.Add($"Grp Size \"{Get(25)}\" is not a number");

            // Student ID max 25 chars
            var studentId = Get(6);
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

    // SESIS PARSE
    // Data starts row 2, headers on row 1
    // Skip if required cols null: 1,2,3,5,9,13,15,19,21,24,25,28,30,31,32,34,35,36,41,42
    // Also skip if col 30 doesn't contain "SERVICE PROVIDED" (case-insensitive)
    // Row display number = i - 1
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

            var required = new[] { 1, 2, 3, 5, 13, 25, 28, 30, 31, 32, 34, 35, 36, 41, 42 };
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

    // VENDOR PORTAL PARSE
    // Data starts row 2, headers on row 1
    // Skip if col 23 (Assign_Id) is null
    // Deduplicate by Assign_Id using a HashSet
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

    // PAYMENTS PARSE
    // Data starts row 2, headers on row 1
    // Skip if col 1 (Voucher) is null
    // This is an UPDATE operation, not insert
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
            decimal? GetDecimal(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetValue<decimal>(); }
                catch
                {
                    var raw = cell.GetValue<string>()?.Trim().Replace("$", "").Replace(",", "");
                    return decimal.TryParse(raw, out var d) ? d : (decimal?)null;
                }
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
                ["Invoice #"] = Get(2),
                ["Batch ID"] = Get(3),
                ["Student ID"] = Get(7),
                ["SSN"] = Get(9),
                ["Provider"] = Get(10),
                ["Subtype"] = Get(11),
                ["Amount"] = GetDecimal(14)?.ToString("C2"),
                ["Date of Service"] = GetDate(15)?.ToString("MM/dd/yyyy"),
                ["Start Time"] = GetTime(16),
                ["IVR Confirm"] = Get(18),
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

    // COMMIT

    public async Task<ImportCommitResult> CommitAsync(ImportType type, ImportPreviewResult preview, CancellationToken ct = default)
    {
        _logger.LogInformation("Committing {Type} import: {FileName} ({ValidRows} rows to process)",
            type, preview.FileName, preview.ValidRows.Count);

        var result = type switch
        {
            ImportType.Mandates => await CommitMandatesAsync(preview, ct),
            ImportType.Sesis => await CommitSesisAsync(preview, ct),
            ImportType.VendorPortal => await CommitVendorPortalAsync(preview, ct),
            ImportType.Payments => await CommitPaymentsAsync(preview, ct),
            ImportType.EvalPayments => await CommitEvalPaymentsAsync(preview, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        // Write ImportLog
        string prefix = type switch
        {
            ImportType.Mandates => "[Approvals]",
            ImportType.Sesis => "[Provider Billing]",
            ImportType.VendorPortal => "[Vendor Portal]",
            ImportType.Payments => "[Voucher Payments]",
            ImportType.EvalPayments => "[Evaluation Voucher Payments]",
            _ => "[Unknown]"
        };

        bool isPaymentType = type is ImportType.Payments or ImportType.EvalPayments;

        string importRecord;
        if (isPaymentType)
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

        if (result.WarningRows.Any())
        {
            importRecord += " Rate Warnings (rows): " + string.Join("; ", result.WarningRows.Select(w => w.RowNumber)) + ";";
        }

        if (isPaymentType)
            _logger.LogInformation("Commit complete for {FileName}: {Updated} row(s) updated, {Skipped} no match",
                preview.FileName, result.Updated, result.Skipped);
        else
            _logger.LogInformation("Commit complete for {FileName}: {Inserted} inserted, {Skipped} skipped",
                preview.FileName, result.Inserted, result.Skipped);

        await _importLogService.CreateAsync(new ImportLogDTO
        {
            FileName = $"{prefix} {preview.FileName}",
            ImportRecord = importRecord,
            ImportDate = DateTime.Now
        }, ct);

        return result;
    }

    // COMMIT MANDATES
    private async Task<ImportCommitResult> CommitMandatesAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        await using var db = _factory.CreateDbContext();
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
                    if (cell.Value.IsText)
                    {
                        if (DateTime.TryParse(cell.GetValue<string>(), out var dt)) return dt;
                        return null;
                    }
                    try { return cell.GetDateTime(); } catch { return null; }
                }

                string studentId = Get(6)!;
                string serviceType = Get(21)!;
                string remainingFreq = Get(29)!;
                string dur = Get(27)!;
                string grpSize = Get(25)!;
                string mandateId = Get(43)!;
                string provider = Get(32)!;

                // Calculate MandateStart / MandateEnd (needed for duplicate check)
                DateTime? firstAttendDate = GetDate(41) ?? DateTime.Now;
                DateTime mandateStart = firstAttendDate.Value.Date;
                DateTime mandateEnd;

                // Duplicate check - Provider included so a reassignment to a new provider
                // creates a new record rather than being skipped.
                bool exists = await db.Mandates.AnyAsync(m =>
                    m.Student_ID == studentId &&
                    m.Service_Type == serviceType &&
                    m.Remaining_Freq == remainingFreq &&
                    m.Dur == dur &&
                    m.Grp_Size == grpSize &&
                    m.Mandate_ID == mandateId &&
                    m.Provider == provider &&
                    m.MandateStart == mandateStart, ct);

                if (exists)
                {
                    skippedRowNumbers.Add(i - 3);
                    continue;
                }
                int month = mandateStart.Month;
                if (month == 7)
                    mandateEnd = new DateTime(mandateStart.Year, 8, 31, 23, 59, 0);
                else if (month > 7)
                    mandateEnd = new DateTime(mandateStart.Year + 1, 6, 30, 23, 59, 0);
                else
                    mandateEnd = new DateTime(mandateStart.Year, 6, 30, 23, 59, 0);

                var entity = new Mandate
                {
                    Conf_Date = GetDate(5),
                    Student_ID = studentId,
                    Last_Name = Get(7),
                    First_Name = Get(8),
                    Home_District = Get(9)?.ToString(),
                    CSE = Get(10),
                    CSE_District = Get(11)?.ToString(),
                    Grade = Get(12),
                    Date_of_Birth = GetDate(13),
                    Admin_DBN = Get(14),
                    D75 = Get(19),
                    Service_Type = serviceType,
                    Lang = Get(23),
                    Grp_Size = grpSize,
                    Dur = dur,
                    Service_Location = Get(28),
                    Remaining_Freq = remainingFreq,
                    Provider = Get(32),
                    Service_Start_Date = GetDate(39),
                    First_Attend_Date = firstAttendDate,
                    Mandate_ID = mandateId,
                    Primary_Contact_Phone_1 = Get(50),
                    Primary_Contact_Phone_2 = Get(51),
                    IEP_Type = Get(3),
                    School_Name = Get(16),
                    Agency_Name = Get(31),
                    Auth_Physical_DBN = Get(36),
                    Assignment_ID = Get(44),
                    Parent_First_Name = Get(47),
                    Parent_Last_Name = Get(48),
                    Parent_Email = Get(49),
                    MandateStart = mandateStart,
                    MandateEnd = mandateEnd,
                    FileName = preview.FileName,
                    RowNumber = i
                };

                db.Mandates.Add(entity);
                await db.SaveChangesAsync(ct);

                int newEntryId = entity.Entry_Id;

                _logger.LogInformation("Mandate {EntryId} created for student {StudentId} from row {Row}",
                    newEntryId, studentId, i - 3);

                // Backfill unlinked Sesis rows that match this mandate
                if (int.TryParse(dur.Split(' ')[0], out int durInt))
                {
                    int grpSizeInt = int.TryParse(grpSize, out var g) ? g : 0;

                    var matchingSesis = await db.Seses
                        .Where(s => s.Entry_Id == null &&
                                    s.Service_Type!.Trim() == serviceType.Trim() &&
                                    s.Student_ID!.Trim() == studentId.Trim() &&
                                    s.date_of_Service >= mandateStart &&
                                    s.date_of_Service <= mandateEnd)
                        .ToListAsync(ct);

                    int backfillCount = 0;
                    foreach (var sesi in matchingSesis)
                    {
                        if (!int.TryParse(sesi.Duration, out int sesiDur)) continue;
                        if (sesiDur != durInt) continue;

                        int actualSize = int.TryParse(sesi.Actual_Size?.TrimStart('0'), out var a) ? a : 1;
                        bool groupMatch = (grpSizeInt == 1 && actualSize == 1) ||
                                         (grpSizeInt > 1 && grpSizeInt >= actualSize);
                        if (!groupMatch) continue;

                        sesi.Entry_Id = newEntryId;
                        backfillCount++;
                    }

                    await db.SaveChangesAsync(ct);

                    if (backfillCount > 0)
                        _logger.LogInformation("Backfilled Entry_Id {EntryId} onto {Count} sesi record(s)", newEntryId, backfillCount);
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

    // COMMIT SESIS
    // Bulk lookups upfront, then batch insert every 500 rows
    private async Task<ImportCommitResult> CommitSesisAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        await using var db = _factory.CreateDbContext();
        using var workbook = new XLWorkbook(new MemoryStream(preview.FileBytes));
        var ws = workbook.Worksheet(1);

        int inserted = 0;
        var skippedRowNumbers = new List<int>();
        skippedRowNumbers.AddRange(preview.SkippedRows.Select(r => r.RowNumber));
        var warningRows = new List<ImportRowWarning>();

        // Bulk lookups

        // 1. Existing duplicate keys: StudentId|ServiceType|DOSDate|StartTime|EndTime|ActualSize
        // Provider name intentionally excluded - name changes between imports would break deduplication.
        var existingKeys = await db.Seses
            .Where(s => s.date_of_Service.HasValue)
            .Select(s => s.Student_ID + "|" + s.Service_Type + "|" +
                         s.date_of_Service!.Value.Date.ToString("yyyyMMdd") + "|" +
                         s.Start_Time + "|" + s.End_Time + "|" + s.Actual_Size)
            .ToHashSetAsync(ct);

        // 2. All providers: "LastName,FirstName" -> Provider_Id
        // Use GroupBy to handle duplicate names gracefully (takes the first match)
        var providerDict = (await db.Providers
            .Select(p => new { Key = (p.LastName ?? "").Trim() + "," + (p.FirstName ?? "").Trim(), p.Provider_Id })
            .ToListAsync(ct))
            .GroupBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (int?)g.First().Provider_Id, StringComparer.OrdinalIgnoreCase);

        // 3. All mandates for in-memory matching
        var allMandates = await db.Mandates
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

        // 3b. Provider_Id → stripped SSN (no dashes) for VendorPortal matching
        var providerSsnDict = await db.Providers
            .Where(p => p.Ssn != null)
            .Select(p => new { p.Provider_Id, Ssn = p.Ssn! })
            .ToDictionaryAsync(p => p.Provider_Id, p => p.Ssn.Replace("-", ""), ct);

        // 3c. Set of (Entry_Id, pSsn) that exist in VendorPortal - used to verify the billing
        // provider is actually linked to the matched mandate before assigning Entry_Id.
        var vpEntryProviders = (await db.VendorPortals
            .Where(v => v.Entry_Id != null && v.pSsn != null)
            .Select(v => new { v.Entry_Id, v.pSsn })
            .Distinct()
            .ToListAsync(ct))
            .Select(v => (EntryId: v.Entry_Id!.Value, PSsn: v.pSsn!))
            .ToHashSet();

        // 4. All active billing rates: "ServiceType|District|Lang" -> Rate
        var billingRateDict = (await db.BillingRates
            .Where(b => b.Active == true)
            .Select(b => new { Key = (b.ServiceType ?? "").Trim() + "|" + (b.District ?? "").Trim() + "|" + (b.Lang ?? "").Trim(), b.Rate })
            .ToListAsync(ct))
            .GroupBy(b => b.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (decimal?)g.First().Rate, StringComparer.OrdinalIgnoreCase);

        // 5. All active provider rates, keyed "ServiceType|District|Lang|ProviderId|GroupSize".
        // GroupSize is blank for the general rate; a size-specific rate is preferred at lookup.
        var providerRateDict = (await db.ProviderRates
            .Where(p => p.Active == true)
            .Select(p => new { p.ServiceType, p.District, p.Lang, p.Provider_Id, p.GroupSize, p.Rate })
            .ToListAsync(ct))
            .GroupBy(p => (p.ServiceType ?? "").Trim() + "|" + (p.District ?? "").Trim() + "|" + (p.Lang ?? "").Trim() + "|" + p.Provider_Id + "|" + (p.GroupSize?.ToString() ?? ""), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (decimal?)g.First().Rate, StringComparer.OrdinalIgnoreCase);

        // Row processing

        int batchSize = _settings.BatchSize;
        var batch = new List<Sesi>(batchSize);

        async Task FlushBatchAsync()
        {
            if (batch.Count == 0) return;
            try
            {
                db.Seses.AddRange(batch);
                await db.SaveChangesAsync(ct);
                inserted += batch.Count;
            }
            catch (Exception ex)
            {
                // Batch failed - fall back to row-by-row so we can skip only the bad ones
                _logger.LogWarning(ex, "CommitSesisAsync: batch of {Count} rows failed, falling back to row-by-row", batch.Count);
                db.ChangeTracker.Clear();
                foreach (var e in batch)
                {
                    try
                    {
                        db.Seses.Add(e);
                        await db.SaveChangesAsync(ct);
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
                            startTime + "|" + endTime + "|" + actualSize;
            if (existingKeys.Contains(dupKey))
            {
                skippedRowNumbers.Add(i - 1);
                continue;
            }

            // Provider_Id lookup
            string provKey = providerLast + "," + providerFirst;
            providerDict.TryGetValue(provKey, out int? providerId);

            // Entry_Id lookup via in-memory mandates + VendorPortal provider check.
            // We only accept a mandate if the billing provider (cols AO/AP) has a VendorPortal
            // entry for it - prevents attaching to another provider's approval ID.
            int? entryId = null;
            if (int.TryParse(duration, out int durInt) && dateOfService.HasValue)
            {
                int actualSizeInt = int.TryParse(actualSize.TrimStart('0'), out var a) ? a : 1;
                if (actualSizeInt == 0) actualSizeInt = 1;

                var candidates = allMandates
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
                    });

                // Only attach to an approval the billing provider (cols AO/AP) is linked to in
                // VendorPortal; otherwise leave Entry_Id null so the row shows as unassigned.
                if (providerId.HasValue && providerSsnDict.TryGetValue(providerId.Value, out var ssnStripped) && !string.IsNullOrEmpty(ssnStripped))
                {
                    // Prefer the approval whose group size matches the session, then the closest
                    // accommodating group, then the most recent.
                    entryId = candidates
                        .Where(m => vpEntryProviders.Contains((m.Entry_Id, ssnStripped)))
                        .OrderBy(m => Math.Abs((int.TryParse(m.Grp_Size, out var mg) ? mg : 0) - actualSizeInt))
                        .ThenByDescending(m => m.MandateStart)
                        .Select(m => (int?)m.Entry_Id)
                        .FirstOrDefault();
                }
            }

            // Billing rate lookup
            string rateKey = serviceType.Trim() + "|" + gDistrict.Trim() + "|" + language.Trim();
            billingRateDict.TryGetValue(rateKey, out decimal? bRate);

            // Provider rate lookup - prefer a rate set for this exact group size, else the general rate
            decimal? pRate = null;
            if (providerId.HasValue)
            {
                int grpForRate = int.TryParse(actualSize.TrimStart('0'), out var gr) && gr > 0 ? gr : 1;
                string pRateBase = serviceType.Trim() + "|" + gDistrict.Trim() + "|" + language.Trim() + "|" + providerId.Value + "|";
                if (!providerRateDict.TryGetValue(pRateBase + grpForRate, out pRate))
                    providerRateDict.TryGetValue(pRateBase, out pRate);
            }

            // Calculate amounts
            int actualSizeForCalc = int.TryParse(actualSize.TrimStart('0'), out var ac) ? ac : 1;
            if (actualSizeForCalc == 0) actualSizeForCalc = 1;
            decimal? bAmount = (bRate.HasValue && durInt > 0) ? bRate.Value * durInt / 60.0m / actualSizeForCalc : null;
            decimal? pAmount = (pRate.HasValue && durInt > 0) ? pRate.Value * durInt / 60.0m / actualSizeForCalc : null;

            // Warn when rates couldn't be resolved so the user knows amounts will be blank
            if (bRate == null || pRate == null)
            {
                var missingParts = new List<string>();
                if (bRate == null) missingParts.Add("missing billing rate");
                if (pRate == null) missingParts.Add("missing provider rate");
                warningRows.Add(new ImportRowWarning { RowNumber = i - 1, Reason = string.Join(" / ", missingParts) });
            }

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
                OverDuration = false,
                UnderGroup = false
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
            SkippedRowNumbers = skippedRowNumbers,
            WarningRows = warningRows
        };
    }

    // COMMIT VENDOR PORTAL
    // Bulk dup check upfront, batch insert every 500, then backfill Entry_Id
    private async Task<ImportCommitResult> CommitVendorPortalAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        await using var db = _factory.CreateDbContext();
        using var workbook = new XLWorkbook(new MemoryStream(preview.FileBytes));
        var ws = workbook.Worksheet(1);

        int inserted = 0;
        var skippedRowNumbers = new List<int>();
        skippedRowNumbers.AddRange(preview.SkippedRows.Select(r => r.RowNumber));

        // Bulk lookups

        // 1. All existing Assign_Ids
        var existingAssignIds = await db.VendorPortals
            .Where(v => v.Assign_Id != null)
            .Select(v => v.Assign_Id!)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

        // 2. All providers: SSN (dashes stripped) -> Provider entity
        var providersBySsn = await db.Providers
            .Where(p => p.Ssn != null)
            .ToListAsync(ct);

        // 3. All mandates for Entry_Id backfill matching
        var allMandates = await db.Mandates
            .Select(m => new
            {
                m.Entry_Id,
                m.Student_ID,
                m.Dur,
                m.Remaining_Freq,
                m.Grp_Size,
                m.MandateStart,
                m.Service_Type
            })
            .ToListAsync(ct);

        // Row processing

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
                db.VendorPortals.AddRange(batch);
                await db.SaveChangesAsync(ct);
                inserted += batch.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CommitVendorPortalAsync: batch of {Count} rows failed, falling back to row-by-row", batch.Count);
                db.ChangeTracker.Clear();
                for (int b = 0; b < batch.Count; b++)
                {
                    var e = batch[b];
                    try
                    {
                        db.VendorPortals.Add(e);
                        await db.SaveChangesAsync(ct);
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

        // Map each Assign_Id to its service subtype (col O) from the file, so the backfill can match
        // on the service of the line itself rather than the provider's profile service.
        var subtypeByAssign = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in preview.ValidRows)
        {
            var aid = ws.Cell(row.RowNumber, 23).GetValue<string>()?.Trim();
            if (string.IsNullOrEmpty(aid) || subtypeByAssign.ContainsKey(aid)) continue;
            subtypeByAssign[aid] = ws.Cell(row.RowNumber, 15).IsEmpty()
                ? null
                : ws.Cell(row.RowNumber, 15).GetValue<string>()?.Trim();
        }

        // Entry_Id backfill pass. Link every still-unlinked assignment that appears in this file -
        // both the rows just inserted AND rows that were skipped as duplicates but never got linked.
        // That covers the case where an assignment was imported before its approval existed: it came
        // in unlinked, and a plain re-import would skip it as a duplicate and never retry the link.
        // Because the assignment is in this file, its col-O service is known, so the service match
        // below stays firm.
        var assignIdsInFile = subtypeByAssign.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newRows = await db.VendorPortals
            .Where(v => v.Entry_Id == null && v.Assign_Id != null && assignIdsInFile.Contains(v.Assign_Id.Trim()))
            .ToListAsync(ct);

        foreach (var vp in newRows)
        {
            if (string.IsNullOrWhiteSpace(vp.pSsn)) continue;

            string pSsn = vp.pSsn;
            string pFreq4 = (vp.pFreq?.Length >= 4) ? vp.pFreq[..4] : (vp.pFreq ?? "");

            // The provider named on the line is authoritative - never link when we can't verify them (#16).
            var provider = providersBySsn
                .FirstOrDefault(p => p.Ssn?.Replace("-", "") == pSsn);
            if (provider == null) continue;

            var candidates = allMandates.Where(m =>
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
            }).ToList();

            // Decide which approval this assignment links to, service first.
            // Preferred signal is the line's own service subtype (col O). When two approvals are
            // identical except service (OT vs Speech), this lands the assignment on the matching one;
            // and because it's the line's service - not the provider's profile - it stays correct even
            // when a provider bills a discipline their profile isn't labelled for. With that reliable
            // signal we're firm: no same-service approval means leave it unassigned (Missing Approval)
            // rather than forcing it onto the wrong one.
            var lineDiscipline = SubtypeDiscipline(subtypeByAssign.GetValueOrDefault(vp.Assign_Id ?? ""));

            var matchedMandate = lineDiscipline != null
                // Firm: match the line's service, or leave unassigned.
                ? candidates.FirstOrDefault(m => ServiceDiscipline(m.Service_Type) == lineDiscipline)
                // No usable subtype on the line - fall back to the provider's profile service, then to
                // any date/duration/frequency/group match, so single-approval cases keep working.
                : ((ServiceDiscipline(provider.ServiceType) is string provDiscipline
                        ? candidates.FirstOrDefault(m => ServiceDiscipline(m.Service_Type) == provDiscipline)
                        : null)
                   ?? candidates.FirstOrDefault());

            if (matchedMandate != null)
                vp.Entry_Id = matchedMandate.Entry_Id;
        }

        int vpBackfillCount = newRows.Count(v => v.Entry_Id != null);
        await db.SaveChangesAsync(ct);

        if (vpBackfillCount > 0)
            _logger.LogInformation("Backfilled Entry_Id onto {Count} vendor portal record(s)", vpBackfillCount);

        // Recalculate overlap/mandate/group flags for all unpaid records
        await db.Database.ExecuteSqlRawAsync("EXEC OverLapMandate", ct);

        return new ImportCommitResult
        {
            Inserted = inserted,
            Skipped = skippedRowNumbers.Count,
            SkippedRowNumbers = skippedRowNumbers
        };
    }

    // COMMIT PAYMENTS
    // One parsed row from a voucher payment file (input to the import matching loop).
    private sealed class PaymentRow
    {
        public int RowNumber { get; set; }
        public string Voucher { get; set; } = "";
        public string? InvoiceNumber { get; set; }
        public string? BatchId { get; set; }
        public string? StudentId { get; set; }
        public string? Ssn { get; set; }
        public string? SsnLast4 { get; set; }
        public string? Provider { get; set; }
        public string? Subtype { get; set; }
        public decimal? Amount { get; set; }
        public DateTime DosDate { get; set; }
        public string StartTimeNormalized { get; set; } = "";
        public string? IvrConfirm { get; set; }
    }

    private sealed class EvalPaymentRow
    {
        public int RowNumber { get; set; }
        public string? StudentId { get; set; }
        public int? SubtypeCode { get; set; }
        public int? EvalYear { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? Amount { get; set; }
        public string? Voucher { get; set; }
    }

    // Maps a payment file service-subtype code (col K) to a discipline keyword + individual/group.
    // Individual codes end in "1" (O1/S1/P1/C1); group codes are two letters (OT/SP/PT/CO).
    private static (string? Discipline, bool IsIndividual, bool Recognized) ParseServiceSubtype(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return (null, false, false);
        var c = code.Trim().ToUpperInvariant();
        string? discipline = c[0] switch
        {
            'O' => "Occupational",
            'S' => "Speech",
            'P' => "Physical",
            'C' => "Counseling",
            _ => null
        };
        if (discipline == null) return (null, false, false);
        bool isIndividual = c.Length >= 2 && c[1] == '1';
        return (discipline, isIndividual, true);
    }

    // The service subtype code on a Vendor Portal line (col O: O1/OT, S1/SP, P1/PT, C1/CO) -> discipline.
    // This is the service of the line itself, so it's a more reliable signal than the provider's
    // profile service (a provider labelled Speech can legitimately bill an OT line). Returns null
    // when the code is missing/unrecognized so the caller can fall back to the provider's service.
    private static string? SubtypeDiscipline(string? subtype)
    {
        if (string.IsNullOrWhiteSpace(subtype)) return null;
        return char.ToUpperInvariant(subtype.Trim()[0]) switch
        {
            'O' => "OT",
            'S' => "SPEECH",
            'P' => "PT",
            'C' => "COUNSELING",
            _ => null
        };
    }

    // Normalizes a free-text service type (from a provider or a mandate) to a discipline, so the
    // two can be compared even when the wording differs ("Speech Therapy" vs "Speech-Language
    // Therapy"). Returns null when it can't tell - callers treat null as "don't block the match".
    private static string? ServiceDiscipline(string? serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType)) return null;
        var s = serviceType.ToUpperInvariant();
        if (s.Contains("OCCUPATIONAL")) return "OT";
        if (s.Contains("SPEECH")) return "SPEECH";
        if (s.Contains("PHYSICAL")) return "PT";
        if (s.Contains("COUNSEL")) return "COUNSELING";
        return null;
    }

    // Updates Sesis rows: sets bPaid = now, Voucher = voucher number
    // Matches on student, date of service, start time, provider SSN last 4, provider name
    private async Task<ImportCommitResult> CommitPaymentsAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        await using var db = _factory.CreateDbContext();
        using var workbook = new XLWorkbook(new MemoryStream(preview.FileBytes));
        var ws = workbook.Worksheet(1);

        int updated = 0;
        int noMatch = 0;
        int deductions = 0;
        var skippedRowNumbers = new List<int>();
        skippedRowNumbers.AddRange(preview.SkippedRows.Select(r => r.RowNumber));

        // Full A-R detail of rows that end up with no matching session, for the #5 export.
        var unmatched = new List<string?[]>();
        var arHeaders = Enumerable.Range(1, 18)
            .Select(c => { var h = ws.Cell(1, c).GetValue<string>()?.Trim(); return string.IsNullOrEmpty(h) ? $"Col {c}" : h; })
            .ToList();

        // Bulk lookups upfront (2 queries total instead of 2 per row)
        // Collect the date range and student IDs from the file first
        var rowData = new List<PaymentRow>();

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
            decimal? GetDecimal(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetValue<decimal>(); }
                catch
                {
                    var raw = cell.GetValue<string>()?.Trim().Replace("$", "").Replace(",", "");
                    return decimal.TryParse(raw, out var d) ? d : (decimal?)null;
                }
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
            rowData.Add(new PaymentRow
            {
                RowNumber = i,
                Voucher = voucher,
                InvoiceNumber = Get(2),
                BatchId = Get(3),
                StudentId = studentId,
                Ssn = ssn,
                SsnLast4 = ssnLast4,
                Provider = provider,
                Subtype = Get(11),
                Amount = GetDecimal(14),
                DosDate = dateOfService.Value.Date,
                StartTimeNormalized = startTimeNormalized,
                IvrConfirm = Get(18),
            });
        }

        if (rowData.Count == 0)
            return new ImportCommitResult { Updated = 0, Skipped = noMatch, SkippedRowNumbers = skippedRowNumbers };

        // Load all relevant Sesis in one query (all students + date range in this file)
        var studentIds = rowData.Select(r => r.StudentId).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var minDate = rowData.Min(r => r.DosDate);
        var maxDate = rowData.Max(r => r.DosDate);

        var allCandidateSesis = await db.Seses
            .Where(s => s.Student_ID != null &&
                        studentIds.Contains(s.Student_ID.Trim()) &&
                        s.date_of_Service.HasValue &&
                        s.date_of_Service.Value.Date >= minDate &&
                        s.date_of_Service.Value.Date <= maxDate)
            .ToListAsync(ct);

        // Load all providers referenced by those Sesis in one query
        var providerIds = allCandidateSesis.Select(s => s.Provider_Id).Where(id => id.HasValue).Distinct().ToList();
        var allProviders = await db.Providers
            .Where(p => providerIds.Contains(p.Provider_Id))
            .ToListAsync(ct);
        var providerById = allProviders.ToDictionary(p => p.Provider_Id);

        // For the col-K approval auto-correct (#4): the student's approvals, plus the
        // Vendor Portal links (Entry_Id <-> provider SSN) so a correction never crosses providers (#16).
        var allMandates = await db.Mandates
            .Where(m => m.Student_ID != null && studentIds.Contains(m.Student_ID.Trim()))
            .ToListAsync(ct);
        var mandatesByStudent = allMandates
            .GroupBy(m => m.Student_ID!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var entryIds = allMandates.Select(m => m.Entry_Id).Distinct().ToList();
        var vpLinks = await db.VendorPortals
            .Where(v => v.pSsn != null && v.Entry_Id.HasValue && entryIds.Contains(v.Entry_Id.Value))
            .Select(v => new { EntryId = v.Entry_Id!.Value, v.pSsn })
            .ToListAsync(ct);
        var linkedEntryIdsBySsn = vpLinks
            .GroupBy(v => v.pSsn!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(x => x.EntryId).ToHashSet(), StringComparer.OrdinalIgnoreCase);

        // Re-point a paid session to the approval indicated by the payment's service subtype (col K),
        // when it differs from what the session is currently linked to. Chooses among the student's
        // approvals matching the subtype's discipline + individual/group and covering the date, and -
        // when we know the provider's Vendor Portal links - only an approval that provider is linked to.
        void AutoCorrectApproval(Sesi sesi, PaymentRow r, Provider prov)
        {
            var (discipline, isIndividual, recognized) = ParseServiceSubtype(r.Subtype);
            if (!recognized || !mandatesByStudent.TryGetValue(r.StudentId!, out var studentMandates)) return;

            HashSet<int>? linkedForProv = null;
            var provSsn = prov.Ssn?.Replace("-", "");
            if (provSsn != null) linkedEntryIdsBySsn.TryGetValue(provSsn, out linkedForProv);

            int sessionSize = int.TryParse(sesi.Actual_Size, out var sz) ? sz : 1;

            var best = studentMandates
                .Where(m => m.Service_Type != null && m.Service_Type.Contains(discipline!, StringComparison.OrdinalIgnoreCase))
                .Where(m => (int.TryParse(m.Grp_Size, out var g) ? g : 1) is var grp && (isIndividual ? grp == 1 : grp > 1))
                .Where(m => m.MandateStart.HasValue && m.MandateEnd.HasValue &&
                            r.DosDate >= m.MandateStart.Value.Date && r.DosDate <= m.MandateEnd.Value.Date)
                .Where(m => linkedForProv == null || linkedForProv.Count == 0 || linkedForProv.Contains(m.Entry_Id))
                .OrderBy(m => Math.Abs((int.TryParse(m.Grp_Size, out var g) ? g : 1) - sessionSize))
                .ThenByDescending(m => m.MandateStart)
                .FirstOrDefault();

            if (best != null && sesi.Entry_Id != best.Entry_Id)
                sesi.Entry_Id = best.Entry_Id;
        }

        // In-memory matching loop
        var paymentRows = new List<Payment>();

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

                // #7: a negative amount is a deduction/clawback. Don't (re)mark the session paid for a
                // clawback - just net it against the running total so the real balance stays visible.
                bool isDeduction = r.Amount.GetValueOrDefault() < 0m;
                if (!isDeduction)
                {
                    sesi.bPaid = DateTime.Now;
                    sesi.Voucher = r.Voucher;
                }
                sesi.VoucherAmount = (sesi.VoucherAmount ?? 0m) + (r.Amount ?? 0m);

                // #4: re-point the session's approval based on the payment's service subtype (col K)
                // when it disagrees. bPaid is now set, so OverLapMandate won't touch it afterward (locked).
                AutoCorrectApproval(sesi, r, prov);
                matchCount++;

                paymentRows.Add(new Payment
                {
                    Voucher = r.Voucher,
                    InvoiceNumber = r.InvoiceNumber,
                    BatchId = r.BatchId,
                    ServiceSubtype = r.Subtype,
                    IvrConfirm = r.IvrConfirm,
                    VoucherAmount = r.Amount,
                    Student_ID = r.StudentId,
                    Ssn = r.Ssn,
                    Provider = r.Provider,
                    date_of_Service = r.DosDate,
                    Start_Time = r.StartTimeNormalized,
                    FileName = preview.FileName,
                    RowNumber = r.RowNumber,
                    Sesis_Id = sesi.Sesis_Id,
                });
            }

            if (matchCount > 0)
            {
                updated += matchCount;
                if (r.Amount.GetValueOrDefault() < 0m) deductions++;
            }
            else
            {
                noMatch++;
                // #5: keep the full A-R detail so it can be exported to Excel for investigation.
                unmatched.Add(Enumerable.Range(1, 18)
                    .Select(c => { var cell = ws.Cell(r.RowNumber, c); return cell.IsEmpty() ? null : cell.GetFormattedString(); })
                    .ToArray());
            }
        }

        // Single save for all changes
        if (updated > 0)
        {
            try
            {
                db.Payments.AddRange(paymentRows);
                await db.SaveChangesAsync(ct);
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
            SkippedRowNumbers = skippedRowNumbers,
            Deductions = deductions,
            UnmatchedHeaders = unmatched.Count > 0 ? arHeaders : new List<string>(),
            UnmatchedRows = unmatched
        };
    }

    // EVALUATION VOUCHER PAYMENTS PARSE (#9)
    // Data starts row 2, headers on row 1. Cols A-T. Key columns:
    //   D(4) OSIS ID, H(8) service subtype, L(12) start date, N(14) payment date,
    //   P(16) amount, Q(17) voucher number.
    private static ImportPreviewResult ParseEvalPayments(IXLWorksheet ws, string fileName, byte[] fileBytes)
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
            decimal? GetDecimal(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetValue<decimal>(); }
                catch
                {
                    var raw = cell.GetValue<string>()?.Trim().Replace("$", "").Replace(",", "");
                    return decimal.TryParse(raw, out var d) ? d : (decimal?)null;
                }
            }

            string? studentId = Get(4);
            string? subtype = Get(8);
            string? voucher = Get(17);

            var preview = new Dictionary<string, string?>
            {
                ["Row #"] = displayRow.ToString(),
                ["Student ID"] = studentId,
                ["Subtype"] = subtype,
                ["Eval Year"] = GetDate(12)?.Year.ToString(),
                ["Payment Date"] = GetDate(14)?.ToString("MM/dd/yyyy"),
                ["Amount"] = GetDecimal(16)?.ToString("C2"),
                ["Voucher"] = voucher,
            };

            if (string.IsNullOrWhiteSpace(voucher))
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = "Missing Voucher number", PreviewColumns = preview });
                continue;
            }
            if (string.IsNullOrWhiteSpace(studentId))
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = "Missing OSIS ID", PreviewColumns = preview });
                continue;
            }
            if (string.IsNullOrWhiteSpace(subtype))
            {
                skipped.Add(new ImportRowResult { RowNumber = displayRow, IsValid = false, SkipReason = "Missing service subtype", PreviewColumns = preview });
                continue;
            }

            valid.Add(new ImportRowResult { RowNumber = i, IsValid = true, PreviewColumns = preview });
        }

        return new ImportPreviewResult { ValidRows = valid, SkippedRows = skipped, FileName = fileName, FileBytes = fileBytes };
    }

    // Pulls the leading DOE numeric code out of a value like "14=Neuropsychological",
    // " 09=OT" or a bare "14" so file col H can be compared to our stored eval service type.
    private static int? LeadingCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Trim().TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var n) ? n : (int?)null;
    }

    // COMMIT EVALUATION VOUCHER PAYMENTS (#8, #9)
    // Matches each row to an existing eval by OSIS (col D) + service subtype code (col H) +
    // the YEAR of the start date (col L) falling on our recorded Evaluation Date (#8 - we keep
    // our date of evaluation and only apply the payment fields). Records payment date (col N),
    // amount (col P) and voucher (col Q); accumulates payments/deductions so the running balance
    // is always right (#7). A row that matches no eval - or two same-subtype evals in one year -
    // is left for manual entry and surfaced in the unmatched export (cols A-T).
    private async Task<ImportCommitResult> CommitEvalPaymentsAsync(ImportPreviewResult preview, CancellationToken ct)
    {
        await using var db = _factory.CreateDbContext();
        using var workbook = new XLWorkbook(new MemoryStream(preview.FileBytes));
        var ws = workbook.Worksheet(1);

        int updated = 0;
        int noMatch = 0;
        int deductions = 0;
        var skippedRowNumbers = new List<int>();
        skippedRowNumbers.AddRange(preview.SkippedRows.Select(r => r.RowNumber));

        // Full A-T detail of rows that match no eval (or are ambiguous), for the unmatched export.
        var unmatched = new List<string?[]>();
        var atHeaders = Enumerable.Range(1, 20)
            .Select(c => { var h = ws.Cell(1, c).GetValue<string>()?.Trim(); return string.IsNullOrEmpty(h) ? $"Col {c}" : h; })
            .ToList();

        var rowData = new List<EvalPaymentRow>();
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
            decimal? GetDecimal(int col)
            {
                var cell = ws.Cell(i, col);
                if (cell.IsEmpty()) return null;
                try { return cell.GetValue<decimal>(); }
                catch
                {
                    var raw = cell.GetValue<string>()?.Trim().Replace("$", "").Replace(",", "");
                    return decimal.TryParse(raw, out var d) ? d : (decimal?)null;
                }
            }

            rowData.Add(new EvalPaymentRow
            {
                RowNumber = i,
                StudentId = Get(4),
                SubtypeCode = LeadingCode(Get(8)),
                EvalYear = GetDate(12)?.Year,
                PaymentDate = GetDate(14),
                Amount = GetDecimal(16),
                Voucher = Get(17),
            });
        }

        if (rowData.Count == 0)
            return new ImportCommitResult { Updated = 0, Skipped = noMatch, SkippedRowNumbers = skippedRowNumbers };

        var studentIds = rowData.Select(r => r.StudentId).Where(s => s != null)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase!)!;

        // Tracked (not AsNoTracking) so the paid amount/voucher updates persist on SaveChanges.
        var candidateEvals = await db.Evals
            .Where(e => e.Student_ID != null && studentIds.Contains(e.Student_ID.Trim())
                        && e.EvalDate.HasValue && e.ServiceType != null)
            .ToListAsync(ct);

        var evalPaymentRows = new List<EvalPayment>();

        foreach (var r in rowData)
        {
            List<Eval> matches = (r.StudentId == null || r.SubtypeCode == null || r.EvalYear == null)
                ? new List<Eval>()
                : candidateEvals
                    .Where(e => string.Equals(e.Student_ID?.Trim(), r.StudentId, StringComparison.OrdinalIgnoreCase)
                                && LeadingCode(e.ServiceType) == r.SubtypeCode
                                && e.EvalDate!.Value.Year == r.EvalYear)
                    .ToList();

            // Exactly one eval must match. Zero (no eval) or more than one (same subtype twice in a
            // year) is left for manual entry per the client, and captured in the unmatched export.
            if (matches.Count == 1)
            {
                var eval = matches[0];
                bool isDeduction = r.Amount.GetValueOrDefault() < 0m;

                if (!isDeduction)
                {
                    eval.bPaid = r.PaymentDate;   // col N - payment date (we keep EvalDate untouched, #8)
                    eval.Voucher = r.Voucher;     // col Q
                }
                eval.VoucherAmount = (eval.VoucherAmount ?? 0m) + (r.Amount ?? 0m);

                evalPaymentRows.Add(new EvalPayment
                {
                    Eval_Id = eval.Eval_Id,
                    Voucher = r.Voucher,
                    ServiceSubtype = r.SubtypeCode?.ToString(),
                    PaymentDate = r.PaymentDate,
                    Amount = r.Amount,
                    FileName = preview.FileName,
                    RowNumber = r.RowNumber,
                    CreatedOn = DateTime.Now,
                });

                updated++;
                if (isDeduction) deductions++;
            }
            else
            {
                noMatch++;
                unmatched.Add(Enumerable.Range(1, 20)
                    .Select(c => { var cell = ws.Cell(r.RowNumber, c); return cell.IsEmpty() ? null : cell.GetFormattedString(); })
                    .ToArray());
            }
        }

        if (updated > 0)
        {
            try
            {
                db.EvalPayments.AddRange(evalPaymentRows);
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CommitEvalPaymentsAsync: failed to save {Count} eval payment updates", updated);
                throw;
            }
        }

        return new ImportCommitResult
        {
            Updated = updated,
            Skipped = noMatch,
            SkippedRowNumbers = skippedRowNumbers,
            Deductions = deductions,
            UnmatchedHeaders = unmatched.Count > 0 ? atHeaders : new List<string>(),
            UnmatchedRows = unmatched
        };
    }

    // ARCHIVE
    public async Task ArchiveFileAsync(ImportType type, string fileName, byte[] fileBytes)
    {
        string basePath = type switch
        {
            ImportType.Mandates => _settings.MandatesArchivePath,
            ImportType.Sesis => _settings.SesisArchivePath,
            ImportType.VendorPortal => _settings.VendorPortalArchivePath,
            ImportType.Payments => _settings.PaymentsArchivePath,
            ImportType.EvalPayments => _settings.EvalPaymentsArchivePath,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        if (string.IsNullOrWhiteSpace(basePath)) return;

        var now = DateTime.Now;
        var destFolder = Path.Combine(basePath, now.Year.ToString(), now.Month.ToString("D2"));
        Directory.CreateDirectory(destFolder);

        var datePrefix = now.ToString("MM.dd.yy");
        var prefixedFileName = $"{datePrefix} {fileName}";
        var destPath = Path.Combine(destFolder, prefixedFileName);

        // Avoid overwriting - append time if file already exists
        if (File.Exists(destPath))
        {
            var name = Path.GetFileNameWithoutExtension(prefixedFileName);
            var ext = Path.GetExtension(prefixedFileName);
            destPath = Path.Combine(destFolder, $"{name}_{now:HHmmss}{ext}");
        }

        await File.WriteAllBytesAsync(destPath, fileBytes);

        _logger.LogInformation("Archived {Type} file to {DestPath}", type, destPath);
    }
}
