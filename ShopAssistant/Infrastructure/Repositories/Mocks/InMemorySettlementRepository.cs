using ShopAssistant.Settlement.Repositories;
using ShopAssistant.Settlement.Entities;
using ShopAssistant.Application.Settlement;
using System.Transactions;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Accounts.Repositories;

namespace ShopAssistant.Infrastructure.Repositories.Mocks;

public class InMemorySettlementRepository : ISettlementRepository
{
    private readonly Dictionary<string, PaymentReference> _paymentReferences = [];
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILedgerEntryRepository _ledgerEntryRepository;

    public InMemorySettlementRepository(IInvoiceRepository invoiceRepository, ILedgerEntryRepository ledgerEntryRepository)
    {
        _invoiceRepository = invoiceRepository;
        _ledgerEntryRepository = ledgerEntryRepository;
    }
    public PaymentReference? GetByRef(string reference)
    {
        if (_paymentReferences.TryGetValue(reference, out var paymentReference))
        {
            return paymentReference;
        }
        
        return null; // Return null if the payment reference is not found
    }

    public void Add(PaymentReference paymentReference)
    {
        _paymentReferences[paymentReference.Reference] = paymentReference;
    }

    public void Save(ProcessedSettlement processedSettlement)
    {
        //TransactionScope transactionScope = new TransactionScope();
        _paymentReferences[processedSettlement.PaymentReference.Reference] = processedSettlement.PaymentReference;
        //_invoiceRepository.Update(processedSettlement.Invoice);
        _ledgerEntryRepository.Add(processedSettlement.LedgerEntry);
        //transactionScope.Complete();
    }
}