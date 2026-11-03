namespace ShopAssistant.Infrastructure.Repositories.Mocks;

using ShopAssistant.Accounts.Entities;
using ShopAssistant.Accounts.Repositories;

public class InMemoryLedgerEntryRepository : ILedgerEntryRepository
{
    private readonly List<LedgerEntry> _ledgerEntries = [];

    public void Add(LedgerEntry ledgerEntry)
    {
        _ledgerEntries.Add(ledgerEntry);
    }

    public IEnumerable<LedgerEntry> GetAll()
    {
        return _ledgerEntries;
    }
}
