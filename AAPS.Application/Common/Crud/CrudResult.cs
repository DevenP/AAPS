namespace AAPS.Application.Common.Crud
{
    public sealed record CrudResult<T>(CrudStatus Status, T? Data = default, string? Message = null);
}
