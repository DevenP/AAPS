namespace AAPS.Application.Common.Crud
{
    public sealed record SaveResult(CrudStatus Status, string? Message = null);
}

