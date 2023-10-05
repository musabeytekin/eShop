namespace Ordering.Domain.AggregatesModel.BuyerAggregate;

public class PaymentMethod : Entity
{
    private string _alias;
    private string _cardNumber;
    private string _cvv;
    private string _cardHolderName;
    private DateTime _expiration;
    private int _cardTypeId;

    public CardType CardType { get; private set; }

    protected PaymentMethod()
    {
    }

    public PaymentMethod(string alias, string cardNumber, string cvv, string cardHolderName, DateTime expiration,
        int cardTypeId)
    {
        _cardNumber = !string.IsNullOrWhiteSpace(cardNumber)
            ? cardNumber
            : throw new ArgumentNullException(nameof(cardNumber));
        _cvv = !string.IsNullOrWhiteSpace(cvv) ? cvv : throw new ArgumentNullException(nameof(cvv));
        _cardHolderName = !string.IsNullOrWhiteSpace(cardHolderName)
            ? cardHolderName
            : throw new ArgumentNullException(nameof(cardHolderName));

        if (expiration < DateTime.UtcNow)
        {
            throw new OrderingDomainException(nameof(expiration));
        }

        _expiration = expiration;
        _cardTypeId = cardTypeId;
        _alias = alias;
    }

    public bool IsEqualTo(int cardTypeId, string cardNumber, DateTime expiration)
    {
        return _cardTypeId == cardTypeId
               && _cardNumber == cardNumber
               && _expiration == expiration;
    }

    
    
}