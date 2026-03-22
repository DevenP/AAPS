namespace AAPS.Application.Abstractions.Services;

public interface IStatementService
{
    void SetLogoPaths(string? headerPath, string? footerPath);
    Task<byte[]> GenerateAsync(string search, Dictionary<string, string> columnFilters, CancellationToken ct = default);
}
