public sealed record SettlementLineFailure(string Reference, string Reason);

public sealed record ReceiveSettlementResult(
    IReadOnlyList<string> PaidOrderIds,
    IReadOnlyList<SettlementLineFailure> Failures)
{
    public bool HasFailures => Failures.Count > 0;
}
