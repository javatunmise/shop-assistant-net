
namespace ShopAssistant.Accounts.Entities;

public class LedgerEntryItem
{
    public required string Description { get; init; }
    public required Money Amount { get; init; }
    public TransactionType EntryType { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string AccountId { get; set; }
}
