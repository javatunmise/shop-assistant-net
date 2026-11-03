namespace ShopAssistant.Settlement.Entities;

public sealed record PaymentReference(
    string Reference,
    decimal Amount,
    string Currency,
    DateTimeOffset Date,
    string Status = "Pending")
{
   
}