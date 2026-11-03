
namespace ShopAssistant.Accounts.Entities;

public class LedgerEntry
{
    public List<LedgerEntryItem> Entries { get; init; } = [];
    public required string DocumentId { get; init; }
    public required string DocumentType { get; init; }
    public Money Amount { get; set; }
    public LedgerEntry(List<LedgerEntryItem> entries, string currency)
    {
        ArgumentNullException.ThrowIfNull(entries, nameof(entries));
        if (entries.Count == 0)
            throw new InvalidOperationException("A ledger entry must contain at least one debit and one credit.");

        var debit = entries.Where(e => e.EntryType == TransactionType.Debit).Sum(e => e.Amount.Value);
        var credit =  entries.Where(e => e.EntryType == TransactionType.Credit).Sum(e => e.Amount.Value);
        if(debit - credit != 0)
            throw new ArgumentException("Account entries should balance to zero");


        Entries = entries;
        Amount = new Money(entries.Where(e => e.EntryType == TransactionType.Debit).Sum(e => e.Amount.Value), currency);
    }
}
public enum TransactionType
{
    Debit, Credit
}