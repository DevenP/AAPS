namespace AAPS.Web.State;

// Remembers the semester the user picked as they move between pages within a
// session (scoped = per Blazor circuit). Null means "not yet chosen" so the grid
// can fall back to the current semester on first load; "All" means no date filter.
public class SemesterState
{
    public const string All = "All";

    public string? SelectedCode { get; set; }
}
