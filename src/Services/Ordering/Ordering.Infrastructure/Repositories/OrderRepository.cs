namespace Ordering.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderingDbContext _context;

    public OrderRepository(OrderingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IUnitOfWork UnitOfWork => _context;

    public Order Add(Order order)
    {
        return _context.Orders.Add(order).Entity;
    }

    public void Update(Order order)
    {
        _context.Entry(order).State = EntityState.Modified;
    }

    public async Task<Order> GetAsync(int orderId)
    {
        // explicit loading for Address
        var order = await _context.Orders.Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            order = _context.Orders.Local.FirstOrDefault(o => o.Id == orderId);
        }

        if (order != null)
        {
            await _context.Entry(order)
                .Collection(o => o.OrderItems).LoadAsync();

            await _context.Entry(order)
                .Reference(o => o.OrderStatus).LoadAsync();
        }

        return order;
    }
}