using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopAssistant.Invoices;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Accounts.Entities;
using ShopAssistant.Application.Payments;

namespace shopassistantweb.Pages.Cashier;

public class IndexModel : PageModel
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ReceivePaymentFromPOSUseCase _receivePaymentUseCase;

    public List<Invoice> Invoices { get; set; } = [];

    [BindProperty]
    public string ItemName { get; set; } = "Pen";

    [BindProperty]
    public decimal ItemAmount { get; set; } = 100m;

    [BindProperty]
    public string Currency { get; set; } = "NGN";

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    [TempData]
    public string? LastPaidInvoiceId { get; set; }

    public IndexModel(
        IInvoiceRepository invoiceRepository,
        ReceivePaymentFromPOSUseCase receivePaymentUseCase)
    {
        _invoiceRepository = invoiceRepository;
        _receivePaymentUseCase = receivePaymentUseCase;
    }

    public void OnGet()
    {
        LoadInvoices();
    }

    public IActionResult OnPostCreateAndPay()
    {
        if (ItemAmount <= 0)
        {
            ErrorMessage = "Amount must be greater than zero.";
            return RedirectToPage();
        }

        if (string.IsNullOrWhiteSpace(ItemName))
        {
            ErrorMessage = "Item description cannot be empty.";
            return RedirectToPage();
        }

        try
        {
            var count = _invoiceRepository.GetAll().Count();
            var newId = $"INV-{(count + 1):D3}";

            var invoice = new Invoice
            {
                Id = newId,
                Description = ItemName.Trim(),
                Amount = new Money(ItemAmount, Currency.Trim().ToUpperInvariant())
            };

            // 1. Add invoice to repository
            _invoiceRepository.Add(invoice);

            // 2. Execute the Cashier Payment Use Case
            _receivePaymentUseCase.Execute(invoice.Id, invoice.Amount.Value);

            StatusMessage = $"Payment of {invoice.Amount.Currency} {invoice.Amount.Value:N2} captured successfully for invoice {invoice.Id}!";
            LastPaidInvoiceId = invoice.Id;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Payment Processing Failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public IActionResult OnPostCreateOnly()
    {
        if (ItemAmount <= 0)
        {
            ErrorMessage = "Amount must be greater than zero.";
            return RedirectToPage();
        }

        try
        {
            var count = _invoiceRepository.GetAll().Count();
            var newId = $"INV-{(count + 1):D3}";

            var invoice = new Invoice
            {
                Id = newId,
                Description = ItemName.Trim(),
                Amount = new Money(ItemAmount, Currency.Trim().ToUpperInvariant())
            };

            _invoiceRepository.Add(invoice);
            StatusMessage = $"Invoice {invoice.Id} created with status Pending Payment.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Invoice creation failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public IActionResult OnPostPayExisting(string invoiceId)
    {
        try
        {
            var invoice = _invoiceRepository.GetById(invoiceId);
            if (invoice == null)
            {
                ErrorMessage = $"Invoice {invoiceId} was not found.";
                return RedirectToPage();
            }

            // Execute the Cashier Use Case
            _receivePaymentUseCase.Execute(invoice.Id, invoice.Amount.Value);

            StatusMessage = $"POS Payment of {invoice.Amount.Currency} {invoice.Amount.Value:N2} captured for invoice {invoice.Id}!";
            LastPaidInvoiceId = invoice.Id;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to receive payment for invoice {invoiceId}: {ex.Message}";
        }

        return RedirectToPage();
    }

    private void LoadInvoices()
    {
        Invoices = _invoiceRepository.GetAll().OrderByDescending(i => i.Id).ToList();
    }
}

