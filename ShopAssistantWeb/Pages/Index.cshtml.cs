using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Settlement.Repositories;
using ShopAssistant.Accounts.Entities;

namespace shopassistantweb.Pages;

public class IndexModel : PageModel
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILedgerEntryRepository _ledgerEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISettlementRepository _settlementRepository;

    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public int TotalLedgerEntries { get; set; }
    public decimal CashBalance { get; set; }
    public decimal ReceivablesBalance { get; set; }
    public decimal RevenueBalance { get; set; }

    public IndexModel(
        IInvoiceRepository invoiceRepository,
        ILedgerEntryRepository ledgerEntryRepository,
        IAccountRepository accountRepository,
        ISettlementRepository settlementRepository)
    {
        _invoiceRepository = invoiceRepository;
        _ledgerEntryRepository = ledgerEntryRepository;
        _accountRepository = accountRepository;
        _settlementRepository = settlementRepository;
    }

    public void OnGet()
    {
        var invoices = _invoiceRepository.GetAll().ToList();
        TotalInvoices = invoices.Count;
        PaidInvoices = invoices.Count(i => i.IsPaid);
        UnpaidInvoices = invoices.Count(i => !i.IsPaid);

        var ledgerEntries = _ledgerEntryRepository.GetAll().ToList();
        TotalLedgerEntries = ledgerEntries.Count;

        // Calculate balances
        var cashAcc = _accountRepository.GetByName("Cash");
        var posAcc = _accountRepository.GetByName("POS Receivables Account");
        var revAcc = _accountRepository.GetByName("Sales Revenue Account");

        CashBalance = CalculateBalance(cashAcc.Id, isDebitNormal: true);
        ReceivablesBalance = CalculateBalance(posAcc.Id, isDebitNormal: true);
        RevenueBalance = CalculateBalance(revAcc.Id, isDebitNormal: false);
    }

    private decimal CalculateBalance(string accountId, bool isDebitNormal)
    {
        var entries = _ledgerEntryRepository.GetAll()
            .SelectMany(e => e.Entries)
            .Where(sub => sub.AccountId == accountId);

        decimal debitSum = entries.Where(e => e.EntryType == TransactionType.Debit).Sum(e => e.Amount.Value);
        decimal creditSum = entries.Where(e => e.EntryType == TransactionType.Credit).Sum(e => e.Amount.Value);

        return isDebitNormal ? (debitSum - creditSum) : (creditSum - debitSum);
    }
}
