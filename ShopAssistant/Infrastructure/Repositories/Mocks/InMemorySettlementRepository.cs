using ShopAssistant.Settlement.Repositories;

namespace ShopAssistant.Infrastructure.Repositories.Mocks;

public class InMemorySettlementRepository : ISettlementRepository
{
    private readonly Dictionary<string, PaymentReference> _paymentReferences = [];

    public PaymentReference? GetByRef(string reference)
    {
        if (_paymentReferences.TryGetValue(reference, out var paymentReference))
        {
            return paymentReference;
        }
        
        return null; // Return null if the payment reference is not found
    }

    public void Add(PaymentReference paymentReference)
    {
        _paymentReferences[paymentReference.Reference] = paymentReference;
    }
}