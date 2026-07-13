namespace AAPS.Application.Abstractions.Services;

public record GeneratedStatementFile(string FileName, byte[] Bytes);

public interface IStatementService
{
    void SetLogoPaths(string? headerPath, string? footerPath);
    Task<List<GeneratedStatementFile>> GenerateAsync(string search, Dictionary<string, string> columnFilters, IList<int>? selectedIds = null, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default);
}
