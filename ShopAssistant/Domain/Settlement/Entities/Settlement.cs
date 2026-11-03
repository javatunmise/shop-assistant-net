namespace ShopAssistant.Settlement.Entities;

public sealed record Settlement(
    decimal TotalAmount,
    IReadOnlyList<PaymentReference> References);
