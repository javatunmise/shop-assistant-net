// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

string exitOption = "7";
var selectedOption = "2";
var menuOperations = new MenuOperations();

while (selectedOption != exitOption)
{
    Console.WriteLine("\nSelect an option:");
    Console.WriteLine("1. Capture Invoice Payment");
    Console.WriteLine("2. Display Ledger Entries");
    Console.WriteLine("4. Process Settlement");
    Console.WriteLine("5. Show Account Balances");
    Console.WriteLine("7. Exit");

    selectedOption = Console.ReadLine();

    switch (selectedOption)
    {
        case "1":
            menuOperations.CapturePayments();
            break;
        case "2":
            menuOperations.ShowLedgerEntries();
            break;
        case "3":
            menuOperations.DisplaySystemState();
            break;
        case "4":
            menuOperations.ReceiveSettlement();
            break;
        case "5":
            menuOperations.ShowAccountBalances();
            break;
        case "7":
            Console.WriteLine("Exiting...");
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }
}
