namespace ShopAssistant.Accounts.Repositories;
public interface IAccountRepository
{
    Account GetByName(string name);
}
