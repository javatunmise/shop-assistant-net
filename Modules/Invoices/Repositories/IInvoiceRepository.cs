namespace ShopAssistant.Invoices.Repositories;
internal interface IInvoiceRepository
{
    Invoice GetById(string id);
    void Add(Invoice invoice);
    IEnumerable<Invoice> GetAll();
}
