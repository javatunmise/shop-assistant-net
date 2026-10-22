
namespace ShopAssistant.Accounts;
class LedgerEntryBuilder 
{
    private readonly List<LedgerEntryItem> entries = [];

    private string DocumentId = "";
    private string DocumentType = "";
    private string _currency = "";

    public LedgerEntryBuilder Document(string documentId, string documentType)
    {
        DocumentId = documentId;
        DocumentType = documentType;
        return this;
    }
    public LedgerEntryBuilder Credit(string accountId, decimal amount, string description = "")
    {
        AddLedgerEntry(new LedgerEntryItem { 
                            Amount = amount, 
                            EntryType = TransactionType.Credit,
                            Description = $"{description}" ,
                            AccountId = accountId
                        });
        return this;
    }

    public LedgerEntryBuilder Debit(string accountId, decimal amount, string description = "")
    {
        AddLedgerEntry(new LedgerEntryItem { 
                            Amount = amount, 
                            EntryType = TransactionType.Debit,
                            Description = $"{description}",
                            AccountId = accountId
                         });
        return this;
    }

    private void AddLedgerEntry(LedgerEntryItem ledgerEntryItem)
    {
        entries.Add(ledgerEntryItem);
    }

    internal LedgerEntry Build()
    {
        var debit = entries.Where(e => e.EntryType == TransactionType.Debit).Sum(e => e.Amount);
        var credit =  entries.Where(e => e.EntryType == TransactionType.Credit).Sum(e => e.Amount);
        if(debit - credit != 0)
            throw new ArgumentException("Account entries should balance to zero");

        return new LedgerEntry(entries, _currency) {
            DocumentId = this.DocumentId,
            DocumentType = this.DocumentType
        };
    }

    internal LedgerEntryBuilder Currency(string currency)
    {
        _currency = currency;
        return this;
    }

    public class LedgerEntry 
    {
        public List<LedgerEntryItem> Entries { get; init; } = [];
        public required string DocumentId { get; init; }
        public required string DocumentType { get; init; }
        public Money Amount { get; set; } 
        public LedgerEntry(List<LedgerEntryItem> entries, string currency)
        {
            ArgumentNullException.ThrowIfNull(entries, nameof(entries));
            Entries = entries;
            Amount = new Money(entries.Where(e => e.EntryType == TransactionType.Debit).Sum(e => e.Amount)) { Currency = currency };
        }
    }

    internal class LedgerEntryItem
    {
        public required string Description { get; init; }
        public decimal Amount { get; init; }
        public TransactionType EntryType { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public required string AccountId { get; set; }
    }
}