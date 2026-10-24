using ShopAssistant.Accounts;
using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Invoices;
using ShopAssistant.Invoices.Repositories;
using Xunit;

public class ReceivePaymentFromPOSUseCaseTests
{
    private const string InvoiceId = "INV-123";

    [Fact]
    public void Execute_WithMatchingAmount_AddsBalancedLedgerEntry()
    {
        var invoiceRepository = new FakeInvoiceRepository(CreateInvoice(100m));
        var accountRepository = new FakeAccountRepository();
        var ledgerRepository = new FakeLedgerEntryRepository();
        var useCase = new ReceivePaymentFromPOSUseCase(
            invoiceRepository,
            accountRepository,
            ledgerRepository);

        useCase.Execute(InvoiceId, 100m);

        var ledgerEntry = Assert.Single(ledgerRepository.Entries);
        Assert.Equal(InvoiceId, invoiceRepository.RequestedId);
        Assert.Equal(InvoiceId, ledgerEntry.DocumentId);
        Assert.Equal("Invoice", ledgerEntry.DocumentType);
        Assert.Equal(new Money(100m) { Currency = "NGN" }, ledgerEntry.Amount);

        Assert.Collection(
            ledgerEntry.Entries,
            debit =>
            {
                Assert.Equal(TransactionType.Debit, debit.EntryType);
                Assert.Equal(100m, debit.Amount);
                Assert.Contains("2", debit.Description);
            },
            credit =>
            {
                Assert.Equal(TransactionType.Credit, credit.EntryType);
                Assert.Equal(100m, credit.Amount);
                Assert.Contains("1", credit.Description);
            });

        Assert.Equal(
            new[] { "POS Receivables Account", "Sales Revenue Account" },
            accountRepository.RequestedNames);
    }

    [Fact]
    public void Execute_WithAmountDifferentFromInvoice_ThrowsAndDoesNotAddLedgerEntry()
    {
        var invoiceRepository = new FakeInvoiceRepository(CreateInvoice(100m));
        var accountRepository = new FakeAccountRepository();
        var ledgerRepository = new FakeLedgerEntryRepository();
        var useCase = new ReceivePaymentFromPOSUseCase(
            invoiceRepository,
            accountRepository,
            ledgerRepository);

        var exception = Assert.Throws<Exception>(() => useCase.Execute(InvoiceId, 99m));

        Assert.Equal("Amount is not equal to invoice amount", exception.Message);
        Assert.Empty(ledgerRepository.Entries);
        Assert.Empty(accountRepository.RequestedNames);
    }

    private static Invoice CreateInvoice(decimal amount) =>
        new()
        {
            Id = InvoiceId,
            Amount = new Money(amount) { Currency = "NGN" }
        };

    private sealed class FakeInvoiceRepository(Invoice invoice) : IInvoiceRepository
    {
        public string? RequestedId { get; private set; }

        public Invoice GetById(string id)
        {
            RequestedId = id;
            return invoice;
        }

        public void Add(Invoice invoice) => throw new NotSupportedException();

        public IEnumerable<Invoice> GetAll() => [invoice];
    }

    private sealed class FakeAccountRepository : IAccountRepository
    {
        private static readonly Dictionary<string, Account> Accounts = new()
        {
            ["POS Receivables Account"] = new() { Id = "1", Name = "POS Receivables Account" },
            ["Sales Revenue Account"] = new() { Id = "2", Name = "Sales Revenue Account" }
        };

        public List<string> RequestedNames { get; } = [];

        public Account GetByName(string name)
        {
            RequestedNames.Add(name);
            return Accounts[name];
        }
    }

    private sealed class FakeLedgerEntryRepository : ILedgerEntryRepository
    {
        public List<LedgerEntryBuilder.LedgerEntry> Entries { get; } = [];

        public void Add(LedgerEntryBuilder.LedgerEntry ledgerEntry) => Entries.Add(ledgerEntry);

        public IEnumerable<LedgerEntryBuilder.LedgerEntry> GetAll() => Entries;
    }
}
