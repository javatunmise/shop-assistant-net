using ShopAssistant.Accounts;
using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Payments.Repositories;
internal class ReceivePaymentFromPOSUseCase(IInvoiceRepository invoiceRepository, 
                                            IAccountRepository accountRepository,
                                            ILedgerEntryRepository ledgerEntryRepository)
{
    private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ILedgerEntryRepository _ledgerEntryRepository = ledgerEntryRepository;

    public void Execute(string invoiceId, decimal amount)
    {
        //find invoice by id 
        var invoice = _invoiceRepository.GetById(invoiceId);
        //check if amount is not greater than invoice amount
        if (amount != invoice.Amount.Value)
        {
            throw new Exception("Amount is not equal to invoice amount");
        }

        var paymentDescription = $"Payment for invoice {invoiceId}";
        var receivableAccount = _accountRepository.GetByName("POS Receivables Account");
        var revenueAccount = _accountRepository.GetByName("Sales Revenue Account");


        var ledgerEntry = new LedgerEntryBuilder()
            .Document(invoiceId, "Invoice")
            .Currency(invoice.Amount.Currency)
            .Credit(revenueAccount.Id, amount, $"{revenueAccount.Name[..1]}: {paymentDescription}")
            .Debit(receivableAccount.Id, amount, $"{receivableAccount.Name[..1]}: {paymentDescription}")
            .Build();

        _ledgerEntryRepository.Add(ledgerEntry);
    }
}

public enum TransactionType
{
    Debit, Credit
}