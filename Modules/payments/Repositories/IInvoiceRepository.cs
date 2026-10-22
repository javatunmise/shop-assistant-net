namespace ShopAssistant.Payments.Repositories;
internal interface IInvoiceRepository
{
    Invoice GetById(string id);
    void Add(Invoice invoice);
}
