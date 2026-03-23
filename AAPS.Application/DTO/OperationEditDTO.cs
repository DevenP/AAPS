namespace AAPS.Application.DTO;

/// <summary>
/// Fields editable via the Operations edit dialog.
/// Null means "leave unchanged" (used for bulk multi-row edits).
/// </summary>
public class OperationEditDTO
{
    public DateTime? DateOfService    { get; set; }
    public string?   StartTime        { get; set; }
    public string?   EndTime          { get; set; }
    public string?   ActualSize       { get; set; }
    public int?      ProviderId       { get; set; }
    public string?   GDistrict        { get; set; }
    public string?   LanguageProvided { get; set; }
    public DateTime? MandateStart     { get; set; }
    public DateTime? MandateEnd       { get; set; }
    public int?      EntryId          { get; set; }

    /// <summary>
    /// When true (single-row edit), ALL fields are written including nulls (null = clear the field).
    /// When false (bulk), only non-null fields are applied.
    /// </summary>
    public bool ApplyAll { get; set; }
}
