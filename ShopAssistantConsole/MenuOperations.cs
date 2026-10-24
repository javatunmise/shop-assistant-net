// See https://aka.ms/new-console-template for more information
using ShopAssistant.Invoices;
using ShopAssistant.Accounts;
using System.Text;
using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Settlement.Repositories;
using ShopAssistant.Infrastructure.Repositories.Mocks;

internal class MenuOperations
{
    private int invoiceId = 1;

    private readonly IInvoiceRepository invoiceRepository = new InMemoryInvoiceRepository();
    private readonly IAccountRepository accountRepository = new InMemoryAccountRepository();
    private readonly ILedgerEntryRepository ledgerRepository = new InMemoryLedgerEntryRepository();
    private readonly ISettlementRepository settlementRepository = new InMemorySettlementRepository();
    private readonly IEnumerable<Account> accounts = [];

    public MenuOperations()
    {
        accounts = accountRepository.GetAll();
    }

    public void DisplaySystemState()
    {
        var revenueAccount = accountRepository.GetByName("Sales Revenue Account");
        Console.WriteLine($"Revenue Account: {revenueAccount.Name}, Id: #{revenueAccount.Id}");
        var receivableAccount = accountRepository.GetByName("POS Receivables Account");
        Console.WriteLine($"Receivable Account: {receivableAccount.Name}, Id: #{receivableAccount.Id}");

        var ledgerEntries = ledgerRepository.GetAll();
        if (!ledgerEntries.Any())
        {
            Console.WriteLine("\nNo ledger entries found.");
            return;
        }

        Console.WriteLine("\n ---------------------- Ledger Entries:  --------------------------- \n");
        foreach (var entry in ledgerEntries)
        {
            Console.WriteLine($"\n Ledger Entry: Document: {entry.DocumentType} #{entry.DocumentId}, Amount: {entry.Amount}");
            Console.WriteLine("                           ---------------------------------------------------");

            foreach (var subEntry in entry.Entries)
            {
                Console.WriteLine($"  Sub Entry: Description: {subEntry.Description}, Type: {subEntry.EntryType},  Amount: {entry.Amount.Currency} {subEntry.Amount}");
            }
        }
    }

    public void ShowLedgerEntries()
    {

        var ledgerEntries = ledgerRepository.GetAll().ToList();
        if (ledgerEntries.Count == 0)
        {
            Console.WriteLine("\nNo ledger entries found.");
            return;
        }

        Console.WriteLine("\n                ---------------------- Ledger Entries:  --------------------------- \n");
        Console.WriteLine("Description".PadRight(40) + " | " + "DR".PadLeft(10) + " | " + "CR".PadLeft(10) + " |  Account");
        Console.WriteLine("".PadLeft(100, '-'));
        var sb = new StringBuilder();
        foreach (var entry in ledgerEntries)
        {
            foreach (var subEntry in entry.Entries)
            {
                sb.AppendFormat("{0} |", subEntry.Description[..Math.Min(subEntry.Description.Length, 40)].PadRight(40));
                sb.AppendFormat("{0} |", subEntry.EntryType == TransactionType.Debit ? subEntry.Amount.ToString().PadLeft(11) : "".PadLeft(11));
                sb.AppendFormat(" {0} |", subEntry.EntryType == TransactionType.Credit ? subEntry.Amount.ToString().PadLeft(10) : "".PadLeft(10));
                sb.AppendFormat("  #{0}: {1}\n", subEntry.AccountId, accounts.FirstOrDefault(a => a.Id == subEntry.AccountId)?.Name ?? "Unknown Account");
                sb.AppendLine("".PadLeft(100, '-'));
            }

            Console.Write(sb.ToString());
            sb.Clear();
        }

    }

    public void ReceiveSettlement()
    {
        var receiveSettlementUseCase = new ReceiveSettlementUseCase(ledgerRepository, accountRepository, invoiceRepository, settlementRepository);

        try
        {
            receiveSettlementUseCase.Execute(new Settlement(300, [.. invoiceRepository.GetAll().Select(i => new PaymentReference(i.Id, i.Amount.Value, i.Amount.Currency, DateTime.UtcNow))]));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Settlement Error: {ex.Message}");
            return;
        }

        Console.WriteLine("Settlement Completed");
    }

    public void ShowAccountBalances()
    {
        var accounts = accountRepository.GetAll();
        Console.WriteLine("\n ---------------------- Account Balances:  --------------------------- \n");

        foreach (var account in accounts)
        {
            Console.WriteLine("".PadLeft(100, '-'));

            Console.WriteLine($"Account No: #{account.Id.PadRight(10)} Account: {account.Name}");
            var ledgerEntries = ledgerRepository.GetAll().Where(e => e.Entries.Any(entry => entry.AccountId == account.Id)).ToList();
            decimal balance = 0;
            foreach (var entry in ledgerEntries)
            {
                foreach (var subEntry in entry.Entries)
                {
                    if (subEntry.AccountId == account.Id)
                    {
                        balance += subEntry.EntryType == TransactionType.Debit ? subEntry.Amount : -subEntry.Amount;
                    }
                }
            }
            Console.WriteLine($"Balance: {balance}");
        }
    }

    public void CapturePayments()
    {
        invoiceId++;
        var items = new[] { Tuple.Create("Pen", 100), Tuple.Create("Book", 200), Tuple.Create("Bag", 300) };
        Console.WriteLine("\n ---------------------- Capturing Payments:  --------------------------- \n");
        Console.WriteLine("Select an item to purchase (e.g., 1, 2, or 3):");
        Console.WriteLine("---------------------------------------------------");

        for (int i = 0; i < items.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {items[i].Item1} - ₦{items[i].Item2}");
        }

        var userItemOption = Console.ReadLine();
        while (userItemOption != "1" && userItemOption != "2" && userItemOption != "3")
        {
            Console.WriteLine("Invalid selection. Please select 1, 2, or 3.");
            userItemOption = Console.ReadKey().KeyChar.ToString();

            Console.WriteLine("\nYour selection: " + userItemOption);
        }

        var item = items[int.Parse(userItemOption) - 1];

        var invoice = new Invoice
        {
            Description = $"Invoice for {item.Item1}",
            Amount = new Money(item.Item2) { Currency = "NGN" },
            Id = $"INV-{invoiceId.ToString("000")}"
        };

        invoiceRepository.Add(invoice);

        var receivePaymentUseCase = new ReceivePaymentFromPOSUseCase(invoiceRepository, accountRepository, ledgerRepository);
        try
        {
            receivePaymentUseCase.Execute(invoice.Id, invoice.Amount.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}