namespace ShopAssistant.Accounts.Entities;

public class Account
{
    public required string Id { get; set; }
    public required string Name { get; set; }
}

public record Money(decimal Value, string Currency)
{
}