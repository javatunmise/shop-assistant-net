using ShopAssistant.Accounts;

namespace ShopAssistant.Payments;
class Invoice
{
    public required string Id { get; init; }
    public required Money Amount { get; init; }
    public string Description { get; set; } = "";
}
