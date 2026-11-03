namespace ShopAssistant.Settlement.Entities;

using ShopAssistant.Accounts.Entities;
using ShopAssistant.Settlement.Entities;

public class ProcessedSettlement(PaymentReference payment, Invoices.Invoice invoice)
{
    public PaymentReference PaymentReference { get; private set; } = new PaymentReference(payment.Reference, payment.Amount, "NGN", DateTime.UtcNow, "Completed"); // Mark invoice as settled
    public Invoices.Invoice Invoice { get; private set; } = invoice;
}
