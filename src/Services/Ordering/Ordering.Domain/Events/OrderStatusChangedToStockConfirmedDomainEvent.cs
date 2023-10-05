namespace Ordering.Domain.Events;

public class OrderStatusChangedToStockConfirmedDomainEvent : INotification
{
    public int OrderId { get; set; }

    public OrderStatusChangedToStockConfirmedDomainEvent(int orderId) => OrderId = orderId;
        
}