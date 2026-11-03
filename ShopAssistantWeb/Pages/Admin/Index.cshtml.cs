using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Settlement.Repositories;
using ShopAssistant.Settlement.Entities;
using ShopAssistant.Application.Settlement;
using ShopAssistant.Accounts.Entities;

namespace shopassistantweb.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerEntryRepository _ledgerEntryRepository;
    private readonly ISettlementRepository _settlementRepository;
    private readonly ReceiveSettlementUseCase _receiveSettlementUseCase;

    public List<AccountBalanceViewModel> AccountBalances { get; set; } = [];
    public List<EligibleSettlementItem> PendingSettlements { get; set; } = [];
    public List<LedgerEntry> LedgerEntries { get; set; } = [];
    public decimal TotalPendingSettlementAmount { get; set; }
    public decimal TotalSettledAmount { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public IndexModel(
        IInvoiceRepository invoiceRepository,
        IAccountRepository accountRepository,
        ILedgerEntryRepository ledgerEntryRepository,
        ISettlementRepository settlementRepository,
        ReceiveSettlementUseCase receiveSettlementUseCase)
    {
        _invoiceRepository = invoiceRepository;
        _accountRepository = accountRepository;
        _ledgerEntryRepository = ledgerEntryRepository;
        _settlementRepository = settlementRepository;
        _receiveSettlementUseCase = receiveSettlementUseCase;
    }

    public void OnGet()
    {
        LoadDashboardData();
    }

    public IActionResult OnPostProcessSettlement()
    {
        try
        {
            var paidInvoices = _invoiceRepository.GetAll()
                .Where(i => i.IsPaid)
                .ToList();

            // Find invoices not yet settled
            var unsettledReferences = paidInvoices
                .Where(i => _settlementRepository.GetByRef(i.Id) == null)
                .Select(i => new PaymentReference(i.Id, i.Amount.Value, i.Amount.Currency, DateTime.UtcNow))
                .ToList();

            if (unsettledReferences.Count == 0)
            {
                StatusMessage = "No pending settlements to process. All paid invoices are already settled.";
                return RedirectToPage();
            }

            decimal totalBatchAmount = unsettledReferences.Sum(r => r.Amount);
            var batch = new SettlementBatch(totalBatchAmount, unsettledReferences);

            // Execute the Admin Use Case
            _receiveSettlementUseCase.Execute(batch);

            StatusMessage = $"Settlement batch processed successfully! Settled {unsettledReferences.Count} transactions totaling NGN {totalBatchAmount:N2}. Cash / Bank credited, POS Receivables cleared.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Settlement execution error: {ex.Message}";
        }

        return RedirectToPage();
    }

    private void LoadDashboardData()
    {
        var accounts = _accountRepository.GetAll().ToList();
        var allEntries = _ledgerEntryRepository.GetAll().ToList();
        LedgerEntries = allEntries.OrderByDescending(e => e.Entries.FirstOrDefault()?.Timestamp ?? DateTime.MinValue).ToList();

        // 1. Compute Account Balances
        AccountBalances = accounts.Select(acc =>
        {
            var relevantSubEntries = allEntries
                .SelectMany(e => e.Entries)
                .Where(sub => sub.AccountId == acc.Id)
                .ToList();

            decimal debits = relevantSubEntries.Where(s => s.EntryType == TransactionType.Debit).Sum(s => s.Amount.Value);
            decimal credits = relevantSubEntries.Where(s => s.EntryType == TransactionType.Credit).Sum(s => s.Amount.Value);

            // Standard normal balance determination
            bool isDebitNormal = acc.Name.Contains("Cash") || acc.Name.Contains("Receivable");
            string normalType = isDebitNormal ? "Debit" : "Credit";
            decimal netBalance = isDebitNormal ? (debits - credits) : (credits - debits);

            return new AccountBalanceViewModel
            {
                AccountId = acc.Id,
                AccountName = acc.Name,
                AccountType = isDebitNormal ? "Asset" : "Revenue",
                NormalBalance = normalType,
                TotalDebits = debits,
                TotalCredits = credits,
                NetBalance = netBalance
            };
        }).ToList();

        // 2. Identify Pending Settlements
        var allInvoices = _invoiceRepository.GetAll().ToList();
        PendingSettlements = allInvoices
            .Where(i => i.IsPaid && _settlementRepository.GetByRef(i.Id) == null)
            .Select(i => new EligibleSettlementItem
            {
                InvoiceId = i.Id,
                Description = i.Description,
                Amount = i.Amount.Value,
                Currency = i.Amount.Currency
            })
            .ToList();

        TotalPendingSettlementAmount = PendingSettlements.Sum(p => p.Amount);

        // 3. Compute Total Settled
        TotalSettledAmount = allInvoices
            .Where(i => _settlementRepository.GetByRef(i.Id) != null)
            .Sum(i => i.Amount.Value);
    }
}

public class AccountBalanceViewModel
{
    public required string AccountId { get; set; }
    public required string AccountName { get; set; }
    public required string AccountType { get; set; }
    public required string NormalBalance { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal NetBalance { get; set; }
}

public class EligibleSettlementItem
{
    public required string InvoiceId { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public required string Currency { get; set; }
}

