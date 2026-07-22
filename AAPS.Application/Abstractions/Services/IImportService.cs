namespace AAPS.Application.Abstractions.Services;

public enum ImportType
{
    Mandates,
    Sesis,
    VendorPortal,
    Payments,
    EvalPayments
}

public record ImportRowResult
{
    public int RowNumber { get; init; }
    public bool IsValid { get; init; }
    public string? SkipReason { get; init; }
    public Dictionary<string, string?> PreviewColumns { get; init; } = new();
}

public record ImportPreviewResult
{
    public List<ImportRowResult> ValidRows { get; init; } = new();
    public List<ImportRowResult> SkippedRows { get; init; } = new();
    public string FileName { get; init; } = "";
    public byte[] FileBytes { get; init; } = Array.Empty<byte>();
}

public record ImportRowWarning
{
    public int RowNumber { get; init; }
    public string Reason { get; init; } = "";
}

public record ImportCommitResult
{
    public int Inserted { get; init; }
    public int Updated { get; init; }
    public int Skipped { get; init; }
    public List<int> SkippedRowNumbers { get; init; } = new();
    public List<ImportRowWarning> WarningRows { get; init; } = new();

    // Payment import: rows that found no matching session, with their full A-R detail
    // for the downloadable unmatched-payments export (#5). Headers come from the file's row 1.
    public List<string> UnmatchedHeaders { get; init; } = new();
    public List<string?[]> UnmatchedRows { get; init; } = new();

    // Payment import: number of matched rows that were deductions/clawbacks (negative amount, #7).
    public int Deductions { get; init; }
}

public interface IImportService
{
    Task<ImportPreviewResult> ParseAsync(ImportType type, string fileName, Stream fileStream);
    Task<ImportCommitResult> CommitAsync(ImportType type, ImportPreviewResult preview, CancellationToken ct = default);
    Task ArchiveFileAsync(ImportType type, string fileName, byte[] fileBytes);
}
