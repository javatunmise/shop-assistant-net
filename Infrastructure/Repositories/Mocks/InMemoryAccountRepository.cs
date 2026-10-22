namespace ShopAssistant.Infrastructure.Repositories.Mocks;

using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Accounts;
internal class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<string, Account> _accounts = new();

    public InMemoryAccountRepository()
    {
        _accounts["POS Receivables Account"] = new Account { Id = "1", Name = "POS Receivables Account" };
        _accounts["Sales Revenue Account"] = new Account { Id = "2", Name = "Sales Revenue Account" };
        _accounts["Cash"] = new Account { Id = "3", Name = "Cash / Bank" };
    }

    public IEnumerable<Account> GetAll()
    {
        return _accounts.Values;
    }

    public Account GetByName(string name)
    {
        if (_accounts.TryGetValue(name, out var account))
        {
            return account;
        }
        throw new KeyNotFoundException($"Account with name {name} not found.");
    }

    public void Add(Account account)
    {
        _accounts[account.Name] = account;
    }
}