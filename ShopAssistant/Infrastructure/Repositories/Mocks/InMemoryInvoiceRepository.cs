
namespace ShopAssistant.Infrastructure.Repositories.Mocks;

using ShopAssistant.Invoices.Repositories;
using ShopAssistant.Invoices;

public class InMemoryInvoiceRepository : IInvoiceRepository
{
    private readonly Dictionary<string, Invoice> _invoices = [];

    public Invoice? GetById(string id)
    {
        if (_invoices.TryGetValue(id, out var invoice))
        {
            return invoice;
        }
        
        return null; // Return null if the invoice is not found
    }

    public void Add(Invoice invoice)
    {
        _invoices[invoice.Id] = invoice;
    }

    public IEnumerable<Invoice> GetAll()
    {
        return _invoices.Values;
    }
}