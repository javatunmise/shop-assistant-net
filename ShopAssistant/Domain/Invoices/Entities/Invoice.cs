using ShopAssistant.Accounts;

namespace ShopAssistant.Invoices;
public class Invoice
{
    public required string Id { get; init; }
    public required Money Amount { get; init; }
    public string Description { get; set; } = "";
    public bool IsPaid { get; set; } = false;
}
