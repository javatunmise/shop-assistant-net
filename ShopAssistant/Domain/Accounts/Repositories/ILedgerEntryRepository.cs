namespace ShopAssistant.Accounts.Repositories;

using ShopAssistant.Accounts;

public interface ILedgerEntryRepository
{
    void Add(LedgerEntryBuilder.LedgerEntry ledgerEntry);
    IEnumerable<LedgerEntryBuilder.LedgerEntry> GetAll();
}