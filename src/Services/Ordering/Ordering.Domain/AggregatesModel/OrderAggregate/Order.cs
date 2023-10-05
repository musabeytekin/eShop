namespace Ordering.Domain.AggregatesModel.OrderAggregate;

// DDD Patterns: Aggregate Roots
// Aggregate Roots are entities that define the boundaries of consistency and transactional boundaries for a given operation.

// we have the Order Aggregate Root, which is the main entity of the Order Aggregate.
// We have to define business rules and invariants for the Order Aggregate Root.
public class Order : Entity, IAggregateRoot
{
    private DateTime _orderDate;

    public Address Address { get; private set; }

    private int? _buyerId;
    public int? GetBuyerId => _buyerId;

    private int _orderStatusId;
    public OrderStatus OrderStatus { get; private set; }

    private string _description;

    private bool _isDraft;

    private readonly List<OrderItem> _orderItems;
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

    private int? _paymentMethodId;

    public static Order NewDraft()
    {
        var order = new Order
        {
            _isDraft = true
        };
        return order;
    }

    protected Order()
    {
        _orderItems = new List<OrderItem>();
        _isDraft = false;
    }

    public Order(string userId, string userName, int cardTypeId, string cardNumber, string cardCvv,
        string cardHolderName, DateTime cardExpiration, DateTime orderDate, int? buyerId, int orderStatusId,
        int? paymentMethodId, Address address) : this()
    {
        _orderDate = DateTime.UtcNow;
        _buyerId = buyerId;
        _orderStatusId = orderStatusId;
        _paymentMethodId = paymentMethodId;
        Address = address;
        
        AddOrderStartedDomainEvent(userId, userName, cardTypeId, cardNumber,
            cardCvv, cardHolderName, cardExpiration);
    }
    
    public void AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
    {
        var existingOrderForProduct = _orderItems
            .SingleOrDefault(o => o.ProductId == productId);
        
        if (existingOrderForProduct != null)
        {
            if (discount > existingOrderForProduct.GetCurrentDiscount())
            {
                existingOrderForProduct.SetNewDiscount(discount);
            }
            
            existingOrderForProduct.AddUnits(units);
        }
        else
        {
            var orderItem = new OrderItem(productId, productName, unitPrice, discount, pictureUrl, units);
            
            _orderItems.Add(orderItem);
        }
    }
    
    public void SetPaymentId(int id)
    {
        _paymentMethodId = id;
    }
    
    public void SetBuyerId(int id)
    {
        _buyerId = id;
    }

    public void SetAwaitingValidationStatus()
    {
        if (_orderStatusId == OrderStatus.Submitted.Id)
        {
            AddDomainEvent(new OrderStatusChangedToAwaitingValidationDomainEvent(Id, _orderItems));
            _orderStatusId = OrderStatus.AwaitingValidation.Id;
        }
    }
    
    public void SetStockConfirmedStatus()
    {
        if (_orderStatusId == OrderStatus.AwaitingValidation.Id)
        {
            AddDomainEvent(new OrderStatusChangedToStockConfirmedDomainEvent(Id));
            _orderStatusId = OrderStatus.StockConfirmed.Id;
            _description = "All the items were confirmed with available stock.";
        }
    }
    
    public void SetPaidStatus()
    {
        if (_orderStatusId == OrderStatus.StockConfirmed.Id)
        {
            AddDomainEvent(new OrderStatusChangedToPaidDomainEvent(Id, OrderItems));

            _orderStatusId = OrderStatus.Paid.Id;
            _description = "The payment was performed at a simulated \"American Bank checking bank account ending on XX35071\"";
        }
    }
    
    public void SetShippedStatus()
    {
        if (_orderStatusId != OrderStatus.Paid.Id)
        {
            StatusChangeException(OrderStatus.Shipped);
        }

        _orderStatusId = OrderStatus.Shipped.Id;
        _description = "The order was shipped.";
        AddDomainEvent(new OrderShippedDomainEvent(this));
    }
    
    public void SetCancelledStatus()
    {
        if (_orderStatusId == OrderStatus.Paid.Id ||
            _orderStatusId == OrderStatus.Shipped.Id)
        {
            StatusChangeException(OrderStatus.Cancelled);
        }

        _orderStatusId = OrderStatus.Cancelled.Id;
        _description = $"The order was cancelled.";
        AddDomainEvent(new OrderCancelledDomainEvent(this));
    }
    
    public void SetCancelledStatusWhenStockIsRejected(IEnumerable<int> orderStockRejectedItems)
    {
        if (_orderStatusId == OrderStatus.AwaitingValidation.Id)
        {
            _orderStatusId = OrderStatus.Cancelled.Id;

            var itemsStockRejectedProductNames = OrderItems
                .Where(c => orderStockRejectedItems.Contains(c.ProductId))
                .Select(c => c.GetOrderItemProductName());

            var itemsStockRejectedDescription = string.Join(", ", itemsStockRejectedProductNames);
            _description = $"The product items don't have stock: ({itemsStockRejectedDescription}).";
        }
    }
        
    private void StatusChangeException(OrderStatus orderStatusToChange)
    {
        throw new OrderingDomainException($"Is not possible to change the order status from {OrderStatus.Name} to {orderStatusToChange.Name}.");
    }
    
    private void AddOrderStartedDomainEvent(string userId, string userName, int cardTypeId, string cardNumber, string cardCvv, string cardHolderName, DateTime cardExpiration)
    {
        var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userId, userName, cardTypeId,
            cardNumber, cardCvv,
            cardHolderName, cardExpiration);
        
        this.AddDomainEvent(orderStartedDomainEvent);
    }
}