using Grpc.Core;

namespace GrpcBasket;

public class BasketService : Basket.BasketBase
{
    private readonly IBasketRepository _repository;
    private readonly ILogger<BasketService> _logger;

    public BasketService(IBasketRepository repository, ILogger<BasketService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public override async Task<CustomerBasketResponse> GetBasketById(BasketRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Begin grpc call from method {Method} for basket id {Id}", context.Method, request.Id);
        
        var data = await _repository.GetBasketAsync(request.Id);
    
        if (data != null)
        {
            context.Status = new Status(StatusCode.OK, $"Basket with id {request.Id} found");
            return MapToCustomerBasketResponse(data);
        }
        else
        {
            context.Status = new Status(StatusCode.NotFound, $"Basket with id {request.Id} not found");
        }

        return new CustomerBasketResponse();
    }
    
    public override async Task<CustomerBasketResponse> UpdateBasket(CustomerBasketRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Begin grpc call BasketService.UpdateBasketAsync for buyer id {BuyerId}", request.BuyerId);

        var customerBasket = MapToCustomerBasket(request);

        var response = await _repository.UpdateBasketAsync(customerBasket);

        if (response != null)
        {
            return MapToCustomerBasketResponse(response);
        }

        context.Status = new Status(StatusCode.NotFound, $"Basket with buyer id {request.BuyerId} do not exist");

        return null;
    }
    
    private CustomerBasketResponse MapToCustomerBasketResponse(CustomerBasket customerBasket)
    {
        var response = new CustomerBasketResponse
        {
            BuyerId = customerBasket.BuyerId
        };

        customerBasket.Items.ForEach(item => response.Items.Add(new BasketItemResponse
        {
            Id = item.Id,
            OldunitPrice = (double)item.OldUnitPrice,
            PictureUrl = item.PictureUrl,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = (double)item.UnitPrice
        }));

        return response;
    }
    
    private CustomerBasket MapToCustomerBasket(CustomerBasketRequest customerBasketRequest)
    {
        var response = new CustomerBasket
        {
            BuyerId = customerBasketRequest.BuyerId
        };

        customerBasketRequest.Items.ToList().ForEach(item => response.Items.Add(new BasketItem
        {
            Id = item.Id,
            OldUnitPrice = (decimal)item.OldunitPrice,
            PictureUrl = item.PictureUrl,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = (decimal)item.UnitPrice
        }));

        return response;
    }

}