
namespace ShopAssistant.Accounts.Entities;

public partial class LedgerEntryBuilder 
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
                            Amount = new Money(amount, _currency), 
                            EntryType = TransactionType.Credit,
                            Description = $"{description}" ,
                            AccountId = accountId
                        });
        return this;
    }

    public LedgerEntryBuilder Debit(string accountId, decimal amount, string description = "")
    {
        AddLedgerEntry(new LedgerEntryItem { 
                            Amount = new Money(amount, _currency), 
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
}
