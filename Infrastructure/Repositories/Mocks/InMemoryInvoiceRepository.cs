
namespace ShopAssistant.Infrastructure.Repositories.Mocks;

using ShopAssistant.Payments.Repositories;
using ShopAssistant.Payments;
internal class InMemoryInvoiceRepository : IInvoiceRepository
{
    private readonly Dictionary<string, Invoice> _invoices = [];

    public Invoice GetById(string id)
    {
        if (_invoices.TryGetValue(id, out var invoice))
        {
            return invoice;
        }
        throw new KeyNotFoundException($"Invoice with ID {id} not found.");
    }

    public void Add(Invoice invoice)
    {
        _invoices[invoice.Id] = invoice;
    }
}