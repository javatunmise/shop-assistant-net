// See https://aka.ms/new-console-template for more information
using ShopAssistant.Payments;
using ShopAssistant.Payments.Repositories;
using ShopAssistant.Accounts;
using ShopAssistant.Infrastructure.Repositories.Mocks;
using System.Text;
using System.Reflection;

Console.WriteLine("Hello, World!");

var invoiceRepository = new InMemoryInvoiceRepository();
var accountRepository = new InMemoryAccountRepository();
var ledgerRepository = new InMemoryLedgerEntryRepository();
string exitOption = "7";
var selectedOption = "2";
var invoiceId = 1;
var accounts = accountRepository.GetAll();

while (selectedOption != exitOption)
{
    Console.WriteLine("\nSelect an option:");
    Console.WriteLine("1. Capture Invoice Payment");
    Console.WriteLine("2. Display Ledger Entries");
    Console.WriteLine("4. Process Settlement");
    Console.WriteLine("7. Exit");

    selectedOption = Console.ReadLine();

    switch (selectedOption)
    {
        case "1":
            CapturePayments();
            break;
        case "2":
            ShowLedgerEntries();
            break;
        case "3":
            DisplaySystemState();
            break;
        case "4":
            ReceiveSettlement();
            break;
        case "7":
            Console.WriteLine("Exiting...");
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }
}

void DisplaySystemState()
{
    var revenueAccount = accountRepository.GetByName("Sales Revenue Account");
    Console.WriteLine($"Revenue Account: {revenueAccount.Name}, Id: #{revenueAccount.Id}");
    var receivableAccount = accountRepository.GetByName("POS Receivables Account");
    Console.WriteLine($"Receivable Account: {receivableAccount.Name}, Id: #{receivableAccount.Id}");

    var ledgerEntries = ledgerRepository.GetAll();
    if(!ledgerEntries.Any())
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

void ShowLedgerEntries() {
    
    var ledgerEntries = ledgerRepository.GetAll().ToList();
    if(!ledgerEntries.Any())
    {
        Console.WriteLine("\nNo ledger entries found.");
        return;
    }

    Console.WriteLine("\n ---------------------- Ledger Entries:  --------------------------- \n");
    Console.WriteLine("Description".PadRight(40) + " | " + "DR".PadLeft(10) + " | " + "CR".PadLeft(10) + " | Account");
    Console.WriteLine("".PadLeft(90, '-'));
    var sb = new StringBuilder();
    foreach (var entry in ledgerEntries)
    {
        foreach (var subEntry in entry.Entries)
        {
            sb.AppendFormat("{0} |", subEntry.Description[..Math.Min(subEntry.Description.Length, 40)].PadRight(40));
            sb.AppendFormat("{0} |", subEntry.EntryType == TransactionType.Debit ? subEntry.Amount.ToString().PadLeft(11) : "".PadLeft(11));
            sb.AppendFormat(" {0} |", subEntry.EntryType == TransactionType.Credit ? subEntry.Amount.ToString().PadLeft(10) : "".PadLeft(10));
            sb.AppendFormat(" #{0}: {1}\n", subEntry.AccountId, accounts.FirstOrDefault(a => a.Id == subEntry.AccountId)?.Name ?? "Unknown Account");
            sb.AppendLine("".PadLeft(90, '-'));
        }

        Console.Write(sb.ToString());
        sb.Clear();
    }

}

void ReceiveSettlement()
{
    var receiveSettlementUseCase = new ReceiveSettlementUseCase(ledgerRepository, accountRepository);
    receiveSettlementUseCase.Execute(new Settlement(300,
        [
            new("INV-001", 100,"NGN", DateTime.UtcNow),
            new("INV-002", 200, "NGN", DateTime.UtcNow),             
        ]));

    Console.WriteLine("Settlement Completed");
}

void CapturePayments()
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