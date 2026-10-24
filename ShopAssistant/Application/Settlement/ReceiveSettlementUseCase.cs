using ShopAssistant.Accounts;
using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Settlement.Repositories;

public class ReceiveSettlementUseCase(ILedgerEntryRepository ledgerEntryRepository, IAccountRepository accountRepository, IInvoiceRepository invoiceRepository, ISettlementRepository settlementRepository)
{
    private readonly ILedgerEntryRepository _ledgerEntryRepository = ledgerEntryRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
    private readonly ISettlementRepository _settlementRepository = settlementRepository;
    public void Execute(Settlement settlement)
    {
        var cashAccount = _accountRepository.GetByName("Cash"); 
        var posReceivablesAccount = _accountRepository.GetByName("POS Receivables Account"); // Replace with actual receivables account ID

        foreach (var payment in settlement.References)
        {
            var existingPayment = _settlementRepository.GetByRef(payment.Reference);
            if (existingPayment != null)
            {
                throw new Exception($"Payment reference {payment.Reference} has already been settled.");
            }
            
            var invoice = _invoiceRepository.GetById(payment.Reference) ?? throw new Exception($"Invoice with id {payment.Reference} not found");
            if (payment.Amount != invoice.Amount.Value)
            {
                throw new Exception($"Payment amount {payment.Amount} does not match invoice amount {invoice.Amount.Value} for invoice {payment.Reference}");
            }

            if(invoice.IsPaid) continue; // Skip already paid invoices

            _settlementRepository.Add(new PaymentReference(payment.Reference, payment.Amount, "", DateTime.UtcNow)); // Mark invoice as settled

            var ledgerEntry = new LedgerEntryBuilder()
                .Document(payment.Reference, "Settlement")
                .Currency(payment.Currency) // Assuming USD for simplicity; adjust as needed
                .Debit(cashAccount.Id, payment.Amount, $"Settlememt: Payment reference: {payment.Reference}")
                .Credit(posReceivablesAccount.Id, payment.Amount, $"Settlememt: Payment reference: {payment.Reference}")
                .Build();

            _ledgerEntryRepository.Add(ledgerEntry);           
        }
    }
}
