namespace ShopAssistant.Accounts.Repositories;

using ShopAssistant.Accounts;

internal interface ILedgerEntryRepository
{
    void Add(LedgerEntryBuilder.LedgerEntry ledgerEntry);
    IEnumerable<LedgerEntryBuilder.LedgerEntry> GetAll();
}