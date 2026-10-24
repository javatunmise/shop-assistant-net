namespace ShopAssistant.Accounts.Repositories;
public interface IAccountRepository
{
    IEnumerable<Account> GetAll();
    Account GetByName(string name);
}
