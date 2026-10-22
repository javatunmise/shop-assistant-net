public sealed record Settlement(
    decimal TotalAmount,
    IReadOnlyList<PaymentReference> References);
