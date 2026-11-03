namespace ShopAssistant.Settlement.Repositories;

using ShopAssistant.Settlement.Entities;

public interface ISettlementRepository
{
    PaymentReference? GetByRef(string reference);
    void Add(PaymentReference paymentReference);
    void Save(ProcessedSettlement processedSettlement);
}