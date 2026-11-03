namespace ShopAssistant.Application.Settlement;

using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Settlement.Repositories;
using ShopAssistant.Settlement.Entities;
using ShopAssistant.Accounts.Entities;
using System.Transactions;

public class ReceiveSettlementUseCase(ILedgerEntryRepository ledgerEntryRepository, IAccountRepository accountRepository, IInvoiceRepository invoiceRepository, ISettlementRepository settlementRepository)
{
    private readonly ILedgerEntryRepository _ledgerEntryRepository = ledgerEntryRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
    private readonly ISettlementRepository _settlementRepository = settlementRepository;

    public void Execute(SettlementBatch batch)
    {
        var cashAccount = _accountRepository.GetByName("Cash");
        var posReceivablesAccount = _accountRepository.GetByName("POS Receivables Account"); // Replace with actual receivables account ID

        foreach (var payment in batch.References)
        {
            var existingPayment = _settlementRepository.GetByRef(payment.Reference);
            if (existingPayment != null)
            {
                continue; // Skip already settled payments
            }

            var invoice = _invoiceRepository.GetById(payment.Reference) ?? throw new Exception($"Invoice with id {payment.Reference} not found");
            if (payment.Amount != invoice.Amount.Value)
            {
                throw new Exception($"Payment amount {payment.Amount} does not match invoice amount {invoice.Amount.Value} for invoice {payment.Reference}");
            }

            var processedSettlement = new ProcessedSettlement(payment, invoice);


            var ledgerEntry = new LedgerEntryBuilder()
                .Document(payment.Reference, "Settlement")
                .Currency(payment.Currency) // Assuming USD for simplicity; adjust as needed
                .Debit(cashAccount.Id, payment.Amount, $"Settlememt: Payment reference: {payment.Reference}")
                .Credit(posReceivablesAccount.Id, payment.Amount, $"Settlememt: Payment reference: {payment.Reference}")
                .Build();

            var treatedSettlement = new PaymentReference(payment.Reference, payment.Amount, payment.Currency, DateTime.UtcNow, "Completed");
            
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            _ledgerEntryRepository.Add(ledgerEntry);
            _settlementRepository.Add(treatedSettlement);
            transactionScope.Complete();
        }
    }
}

class ProcessedSettlement2
{
    public string Status { get; private set; }
    public ProcessedSettlement2(string batchId, string reference, string referenceType, decimal amount, DateTime date)
    {
        Status = "Pending";
    }

    public void MarkAsCompleted()
    {
        Status = "Completed";
    }

    public bool IsComplete => Status == "Completed";
}