namespace ShopAssistant.Application.Settlement;

using ShopAssistant.Accounts.Entities;
using ShopAssistant.Settlement.Entities;

public class ProcessedSettlement
{
    public LedgerEntry LedgerEntry { get; private set; }
    public PaymentReference PaymentReference { get; private set; }
    public Invoices.Invoice Invoice { get; private set; }

    public ProcessedSettlement(Account cashAccount, Account posReceivablesAccount, PaymentReference payment, Invoices.Invoice invoice)
    {
        if (payment.Amount != invoice.Amount.Value)
        {
            throw new Exception($"Payment amount {payment.Amount} does not match invoice amount {invoice.Amount.Value} for invoice {payment.Reference}");
        }

        if (invoice.IsPaid)
        {
            throw new Exception($"Invoice with id {payment.Reference} has already been paid");
        }

        Invoice = invoice;

        PaymentReference = new PaymentReference(payment.Reference, payment.Amount, "", DateTime.UtcNow); // Mark invoice as settled

        LedgerEntry = new LedgerEntryBuilder()
            .Document(payment.Reference, "Settlement")
            .Currency(payment.Currency) // Assuming USD for simplicity; adjust as needed
            .Debit(cashAccount.Id, payment.Amount, $"Settlememt: Payment reference: {payment.Reference}")
            .Credit(posReceivablesAccount.Id, payment.Amount, $"Settlememt: Payment reference: {payment.Reference}")
            .Build();
    }
}
