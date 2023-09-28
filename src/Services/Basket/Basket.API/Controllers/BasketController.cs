namespace Basket.API.Controllers;

[Route("api/v1/[controller]")]
[Authorize]
[ApiController]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _repository;
    private readonly IIdentityService _identityService;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketRepository repository, IIdentityService identityService,
        ILogger<BasketController> logger)
    {
        _repository = repository;
        _identityService = identityService;
        _logger = logger;
    }

    // GET api/v1/[controller]/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerBasket>> GetBasketByIdAsync(string id)
    {
        var basket = await _repository.GetBasketAsync(id);
        return Ok(basket ?? new CustomerBasket(id));
    }
    
    [HttpPost]
    public async Task<ActionResult<CustomerBasket>> UpdateBasketAsync([FromBody] CustomerBasket value)
    {
        return Ok(await _repository.UpdateBasketAsync(value));
    }

    // POST api/v1/[controller]/checkout
    [HttpPost]
    [Route("checkout")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CheckoutAsync([FromBody] BasketCheckout basketCheckout,
        [FromHeader(Name = "x-requestid")] string requestId)
    {
        var userId = _identityService.GetUserIdentity();
        
        basketCheckout.RequestId = (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ? guid : basketCheckout.RequestId;
        
        var basket = await _repository.GetBasketAsync(userId);

        if (basket == null)
            return BadRequest();

        // TODO: Send checkout event to rabbitmq
        
        return Accepted();
    }
    
    // DELETE api/values/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task DeleteBasketByIdAsync(string id)
    {
        await _repository.DeleteBasketAsync(id);
    }
}