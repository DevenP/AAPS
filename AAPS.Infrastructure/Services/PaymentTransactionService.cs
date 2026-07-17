using AAPS.Application.Abstractions.Services;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Infrastructure.Services;

public class PaymentTransactionService : IPaymentTransactionService
{
    private const string TypeWriteOff = "Write-Off";
    private const string TypeReturned = "Returned to DOE";
    private const string TypeAdjustment = "Adjustment";

    private readonly IDbContextFactory<AppDbContext> _factory;

    public PaymentTransactionService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task MarkWriteOffAsync(IEnumerable<int> sesisIds, string? note, CancellationToken ct = default)
    {
        var ids = sesisIds.Distinct().ToList();
        if (ids.Count == 0) return;

        await using var db = _factory.CreateDbContext();
        var sesis = await db.Seses.Where(s => ids.Contains(s.Sesis_Id)).ToListAsync(ct);
        var txns = await db.PaymentTransactions.Where(t => ids.Contains(t.Sesis_Id)).ToListAsync(ct);
        var now = DateTime.Now;

        foreach (var s in sesis)
        {
            var remaining = RemainingOwed(s, txns.Where(t => t.Sesis_Id == s.Sesis_Id));
            db.PaymentTransactions.Add(new PaymentTransaction
            {
                Sesis_Id = s.Sesis_Id,
                TxnDate = now,
                TxnType = TypeWriteOff,
                Amount = remaining,
                Note = note,
                CreatedOn = now
            });
            s.AdjustmentStatus = "Loss";
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkReturnedAsync(IEnumerable<int> sesisIds, decimal amount, string? note, CancellationToken ct = default)
    {
        var ids = sesisIds.Distinct().ToList();
        if (ids.Count == 0) return;

        await using var db = _factory.CreateDbContext();
        var sesis = await db.Seses.Where(s => ids.Contains(s.Sesis_Id)).ToListAsync(ct);
        var now = DateTime.Now;

        foreach (var s in sesis)
        {
            db.PaymentTransactions.Add(new PaymentTransaction
            {
                Sesis_Id = s.Sesis_Id,
                TxnDate = now,
                TxnType = TypeReturned,
                Amount = amount,
                Note = note,
                CreatedOn = now
            });
            s.AdjustmentStatus = "Returned";
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task ClearAdjustmentAsync(IEnumerable<int> sesisIds, CancellationToken ct = default)
    {
        var ids = sesisIds.Distinct().ToList();
        if (ids.Count == 0) return;

        await using var db = _factory.CreateDbContext();
        var txns = await db.PaymentTransactions.Where(t => ids.Contains(t.Sesis_Id)).ToListAsync(ct);
        db.PaymentTransactions.RemoveRange(txns);

        var sesis = await db.Seses.Where(s => ids.Contains(s.Sesis_Id)).ToListAsync(ct);
        foreach (var s in sesis) s.AdjustmentStatus = null;

        await db.SaveChangesAsync(ct);
    }

    public async Task<SessionLedgerDTO> GetLedgerAsync(int sesisId, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();

        var s = await db.Seses.AsNoTracking().FirstOrDefaultAsync(x => x.Sesis_Id == sesisId, ct)
                ?? throw new KeyNotFoundException($"Sesi {sesisId} not found.");

        var payments = await db.Payments.AsNoTracking()
            .Where(p => p.Sesis_Id == sesisId)
            .OrderBy(p => p.date_of_Service).ThenBy(p => p.Voucher_Id)
            .ToListAsync(ct);

        var txns = await db.PaymentTransactions.AsNoTracking()
            .Where(t => t.Sesis_Id == sesisId)
            .OrderBy(t => t.TxnDate).ThenBy(t => t.Transaction_Id)
            .ToListAsync(ct);

        var billed = s.bAmount ?? 0m;
        var balance = billed;
        var entries = new List<SessionLedgerEntry>
        {
            new(s.Billed, "Billed", billed, balance, null, null)
        };

        foreach (var p in payments)
        {
            var amt = p.VoucherAmount ?? 0m;
            balance -= amt;                       // payment reduces owed; a negative (deduction) raises it
            entries.Add(new SessionLedgerEntry(
                p.date_of_Service, amt < 0 ? "Deduction" : "Payment", amt, balance, null, p.Voucher));
        }

        foreach (var t in txns)
        {
            balance += t.TxnType switch
            {
                TypeReturned => t.Amount,     // returned to DOE puts the amount back on the books
                _ => -t.Amount                // write-off / adjustment reduce what's owed
            };
            entries.Add(new SessionLedgerEntry(t.TxnDate, t.TxnType, t.Amount, balance, t.Note, null));
        }

        return new SessionLedgerDTO(sesisId, billed, s.AdjustmentStatus, balance, entries);
    }

    // What's still owed to us on a session: billed, less net payments, plus anything returned
    // to the DOE, less prior adjustments and write-offs.
    private static decimal RemainingOwed(Sesi s, IEnumerable<PaymentTransaction> txns)
    {
        var billed = s.bAmount ?? 0m;
        var netPaid = s.VoucherAmount ?? 0m;
        var returned = txns.Where(t => t.TxnType == TypeReturned).Sum(t => t.Amount);
        var adjusted = txns.Where(t => t.TxnType == TypeAdjustment).Sum(t => t.Amount);
        var writtenOff = txns.Where(t => t.TxnType == TypeWriteOff).Sum(t => t.Amount);
        return billed - netPaid + returned - adjusted - writtenOff;
    }
}
