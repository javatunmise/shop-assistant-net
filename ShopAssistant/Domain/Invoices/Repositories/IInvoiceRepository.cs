namespace ShopAssistant.Invoices.Repositories;
public interface IInvoiceRepository
{
    Invoice GetById(string id);
    void Add(Invoice invoice);
    IEnumerable<Invoice> GetAll();
}
