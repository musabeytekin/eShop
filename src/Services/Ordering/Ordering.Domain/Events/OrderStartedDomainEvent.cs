namespace Ordering.Domain.Events;

public class OrderStartedDomainEvent : INotification
{
    public string UserId { get; }
    public string UserName { get; }
    public int CardTypeId { get; }
    public string CardNumber { get; }
    public string CardCvv { get; }
    public string CardHolderName { get; }
    public DateTime CardExpiration { get; }
    public Order Order { get; }

    public OrderStartedDomainEvent(Order order, string userId, string userName,
        int cardTypeId, string cardNumber,
        string cardCvv, string cardHolderName,
        DateTime cardExpiration)
    {
        Order = order;
        UserId = userId;
        UserName = userName;
        CardTypeId = cardTypeId;
        CardNumber = cardNumber;
        CardCvv = cardCvv;
        CardHolderName = cardHolderName;
        CardExpiration = cardExpiration;
    }
}