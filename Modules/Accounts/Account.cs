namespace ShopAssistant.Accounts;

public class Account
{
    public required string Id { get; set; }
    public required string Name { get; set; }
}

public record Money(decimal Value)
{
    public required string Currency { get; init; }
}