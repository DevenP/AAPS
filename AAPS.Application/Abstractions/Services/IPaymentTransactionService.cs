using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IPaymentTransactionService
{
    // Write off the remaining balance on each session as a loss (#6). Records the forgiven
    // amount as a ledger entry and flags the session "Loss" so it drops out of owed totals.
    Task MarkWriteOffAsync(IEnumerable<int> sesisIds, string? note, CancellationToken ct = default);

    // Record funds returned to the DOE on each session (#6/#20) - e.g. billed incorrectly and
    // returning to re-bill. Adds a ledger entry for the returned amount and flags "Returned".
    Task MarkReturnedAsync(IEnumerable<int> sesisIds, decimal amount, string? note, CancellationToken ct = default);

    // Undo the current loss/returned state on each session (removes its adjustment entries).
    Task ClearAdjustmentAsync(IEnumerable<int> sesisIds, CancellationToken ct = default);

    // The full money trail for one session: billed, payments/deductions, and adjustments,
    // with a running balance.
    Task<SessionLedgerDTO> GetLedgerAsync(int sesisId, CancellationToken ct = default);
}
