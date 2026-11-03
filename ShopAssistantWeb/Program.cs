using ShopAssistant.Accounts.Repositories;
using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Settlement.Repositories;
using ShopAssistant.Infrastructure.Repositories.Mocks;
using ShopAssistant.Application.Payments;
using ShopAssistant.Application.Settlement;
using ShopAssistant.Invoices;
using ShopAssistant.Accounts.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register ShopAssistant Repositories as Singletons for in-memory persistence
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddSingleton<IInvoiceRepository, InMemoryInvoiceRepository>();
builder.Services.AddSingleton<ILedgerEntryRepository, InMemoryLedgerEntryRepository>();
builder.Services.AddSingleton<ISettlementRepository, InMemorySettlementRepository>();

// Register the 2 Core Use Cases
builder.Services.AddScoped<ReceivePaymentFromPOSUseCase>();
builder.Services.AddScoped<ReceiveSettlementUseCase>();

var app = builder.Build();

// Seed initial sample data for demonstration
using (var scope = app.Services.CreateScope())
{
    var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
    var paymentUseCase = scope.ServiceProvider.GetRequiredService<ReceivePaymentFromPOSUseCase>();

    // Initial sample invoice 1: Already paid via POS
    var invoice1 = new Invoice
    {
        Id = "INV-001",
        Description = "Pen and Notebook Set",
        Amount = new Money(250m, "NGN")
    };
    invoiceRepo.Add(invoice1);
    try
    {
        paymentUseCase.Execute(invoice1.Id, invoice1.Amount.Value);
    }
    catch
    {
        // Ignore in seed if any
    }

    // Initial sample invoice 2: Unpaid (ready for cashier checkout)
    var invoice2 = new Invoice
    {
        Id = "INV-002",
        Description = "Office Stationery Bag",
        Amount = new Money(300m, "NGN")
    };
    invoiceRepo.Add(invoice2);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
