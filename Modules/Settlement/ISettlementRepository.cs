public interface ISettlementRepository
{
    PaymentReference? GetByRef(string reference);
    void Add(PaymentReference paymentReference);
}