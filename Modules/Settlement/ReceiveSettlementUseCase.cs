using ShopAssistant.Accounts;
using ShopAssistant.Accounts.Repositories;

internal class ReceiveSettlementUseCase(ILedgerEntryRepository ledgerEntryRepository, IAccountRepository accountRepository)
{
    private readonly ILedgerEntryRepository _ledgerEntryRepository = ledgerEntryRepository;
    private readonly IAccountRepository _accountRepository = accountRepository;

    public void Execute(Settlement settlement)
    {
        var cashAccount = _accountRepository.GetByName("Cash"); 
        var posReceivablesAccount = _accountRepository.GetByName("POS Receivables Account"); // Replace with actual receivables account ID
    
        foreach (var payment in settlement.References)
        {
            var ledgerEntry = new LedgerEntryBuilder()
                .Document(payment.Reference, "Settlement")
                .Currency(payment.Currency) // Assuming USD for simplicity; adjust as needed
                .Debit(cashAccount.Id, payment.Amount, $"Payment reference: {payment.Reference}")
                .Credit(posReceivablesAccount.Id, payment.Amount, $"Payment reference: {payment.Reference}")
                .Build();

            _ledgerEntryRepository.Add(ledgerEntry);           
        }
    }

    private static void ValidateBatch(Settlement settlement)
    {
        if (settlement.References is null || settlement.References.Count == 0)
        {
            throw new InvalidSettlementException("Settlement must include at least one payment reference.");
        }

        var sum = settlement.References.Sum(reference => reference.Amount);
        if (sum != settlement.TotalAmount)
        {
            throw new InvalidSettlementException(
                $"Settlement total {settlement.TotalAmount} does not match the sum of payment references {sum}.");
        }
    }    
}
