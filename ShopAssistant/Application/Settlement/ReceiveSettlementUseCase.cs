namespace ShopAssistant.Application.Settlement;

using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Settlement.Repositories;
using ShopAssistant.Settlement.Entities;

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
                continue; // Skip already settled payments
            }

            var invoice = _invoiceRepository.GetById(payment.Reference) ?? throw new Exception($"Invoice with id {payment.Reference} not found");
            if (payment.Amount != invoice.Amount.Value)
            {
                throw new Exception($"Payment amount {payment.Amount} does not match invoice amount {invoice.Amount.Value} for invoice {payment.Reference}");
            }

            if (invoice.IsPaid)
            {
                throw new Exception($"Invoice with id {payment.Reference} has already been paid");
            }
     
            var processedSettlement = new ProcessedSettlement(cashAccount, posReceivablesAccount, payment, invoice);           

            _settlementRepository.Save(processedSettlement);
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