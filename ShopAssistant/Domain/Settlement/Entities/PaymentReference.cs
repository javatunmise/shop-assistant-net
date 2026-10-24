public sealed record PaymentReference(
    string Reference,
    decimal Amount,
    string Currency,
    DateTimeOffset Date);
