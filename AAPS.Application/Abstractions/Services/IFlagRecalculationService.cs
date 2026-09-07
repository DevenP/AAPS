namespace AAPS.Application.Abstractions.Services;

/// <summary>
/// Recomputes the operations alert flags (Overlap / Over Mandate / Over Duration / Under Group)
/// in the background so a save can return to the user immediately. Requests are coalesced - a
/// burst of edits results in a single recompute - and the flags catch up a few seconds later.
/// </summary>
public interface IFlagRecalculationService
{
    /// <summary>Queue a recompute. Returns immediately; the work runs in the background.</summary>
    void Request();

    /// <summary>True while a recompute is running (drives the "recalculating alerts…" indicator).</summary>
    bool IsRunning { get; }

    /// <summary>Raised when a recompute starts or finishes. Fired from a background thread.</summary>
    event Action? StateChanged;
}
