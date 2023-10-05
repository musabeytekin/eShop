namespace Ordering.Domain.AggregatesModel.OrderAggregate;

public class OrderItem : Entity
{
    private string _productName;
    private int _units;
    private decimal _unitPrice;
    private string _pictureUrl;
    private decimal _discount;


    public int ProductId { get; private set; }

    protected OrderItem()
    {
    }
    
    public OrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
    {
        if (units <= 0)
        {
            throw new OrderingDomainException("Invalid number of units");
        }
        
        // total of order item is cannot be lower than applied discount
        if ((unitPrice * units) < discount)
        {
            throw new OrderingDomainException("The total of order item is lower than applied discount");
        }

        ProductId = productId;
        _productName = productName;
        _unitPrice = unitPrice;
        _discount = discount;
        _pictureUrl = pictureUrl;
        _units = units;
    }
    
    public string GetPictureUri() => _pictureUrl;
    
    public decimal GetCurrentDiscount() 
    {
        // some business logic may be here
        return _discount;
    }
    
    public int GetUnits() => _units;
    
    public decimal GetUnitPrice() => _unitPrice;
    
    public string GetOrderItemProductName() => _productName;
    
    public void SetNewDiscount(decimal discount)
    {
        if (discount < 0)
        {
            throw new OrderingDomainException("Discount is not valid");
        }

        _discount = discount;
    }
    
    public void AddUnits(int units)
    {
        if (units < 0)
        {
            throw new OrderingDomainException("Invalid units");
        }

        _units += units;
    }
    
}