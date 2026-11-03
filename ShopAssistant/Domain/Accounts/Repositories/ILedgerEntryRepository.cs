namespace ShopAssistant.Accounts.Repositories;

using ShopAssistant.Accounts.Entities;

public interface ILedgerEntryRepository
{
    void Add(LedgerEntry ledgerEntry);
    IEnumerable<LedgerEntry> GetAll();
}