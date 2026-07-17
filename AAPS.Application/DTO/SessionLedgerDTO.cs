namespace AAPS.Application.DTO;

// One line in a session's money trail (#6/#20).
public record SessionLedgerEntry(
    DateTime? Date,
    string Type,        // Billed | Payment | Deduction | Returned to DOE | Write-Off | Adjustment
    decimal Amount,     // the dollar figure for this line
    decimal Balance,    // running balance still owed to us after this line
    string? Note,
    string? Reference); // voucher / invoice, when it's a payment

public record SessionLedgerDTO(
    int SesisId,
    decimal BilledAmount,
    string? Status,             // null | "Loss" | "Returned"
    decimal CurrentBalance,     // what's still owed to us (0 once written off)
    List<SessionLedgerEntry> Entries);
