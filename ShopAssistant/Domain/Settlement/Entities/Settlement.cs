namespace ShopAssistant.Settlement.Entities;

public sealed record SettlementBatch(
    decimal TotalAmount,
    IReadOnlyList<PaymentReference> References);
