using AAPS.Application.Common.Crud;

namespace AAPS.Application.VendorPortals;

public interface IVendorPortalCrudService
{
    Task<CrudResult<Dictionary<string, object?>>> GetByIdAsync(
        int id,
        CancellationToken ct = default);

    Task<CrudResult<object>> CreateAsync(
        Dictionary<string, object?> values,
        CancellationToken ct = default);

    Task<SaveResult> UpdateAsync(
        int id,
        Dictionary<string, object?> values,
        string? rowVersionBase64,
        CancellationToken ct = default);

    Task<SaveResult> DeleteAsync(
        int id,
        string? rowVersionBase64,
        CancellationToken ct = default);
}
