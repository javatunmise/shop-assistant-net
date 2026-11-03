namespace ShopAssistant.Accounts.Repositories;

using ShopAssistant.Accounts.Entities;
public interface IAccountRepository
{
    IEnumerable<Account> GetAll();
    Account GetByName(string name);
}
