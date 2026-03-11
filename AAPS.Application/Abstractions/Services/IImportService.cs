namespace AAPS.Application.Abstractions.Services;

public enum ImportType
{
    Mandates,
    Sesis,
    VendorPortal,
    Payments
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

public record ImportCommitResult
{
    public int Inserted { get; init; }
    public int Updated { get; init; }
    public int Skipped { get; init; }
    public List<int> SkippedRowNumbers { get; init; } = new();
}

public interface IImportService
{
    Task<ImportPreviewResult> ParseAsync(ImportType type, string fileName, Stream fileStream);
    Task<ImportCommitResult> CommitAsync(ImportType type, ImportPreviewResult preview, CancellationToken ct = default);
    Task ArchiveFileAsync(ImportType type, string fileName, byte[] fileBytes);
}
