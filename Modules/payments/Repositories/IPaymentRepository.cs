public interface IPaymentRepository
{
    void ReceivePayment(string invoiceId, decimal amount);
}