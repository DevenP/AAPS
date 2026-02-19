namespace AAPS.Application.Common.Crud
{
    public enum CrudStatus
    {
        Success,
        NotFound,
        Conflict,        // concurrency conflict
        ValidationError,
        Error
    }
}
