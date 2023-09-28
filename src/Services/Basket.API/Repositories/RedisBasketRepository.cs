using System.Text.Json;
using Services.Common;

namespace Basket.API.Repositories;

public class RedisBasketRepository : IBasketRepository
{
    private readonly ILogger<RedisBasketRepository> _logger;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisBasketRepository(ILogger<RedisBasketRepository> logger, ConnectionMultiplexer redis, IDatabase database)
    {
        _logger = logger;
        _redis = redis;
        _database = database;
    }

    public async Task<CustomerBasket?> GetBasketAsync(string customerId)
    {
        var data = await _database.StringGetAsync(customerId);

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<CustomerBasket>(data, JsonDefaults.CaseInsensitiveOptions);
    }
    

    public IEnumerable<string>? GetUsers()
    {
        var server = GetServer();
        var data = server.Keys();

        return data?.Select(k => k.ToString());
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
    {
        var created = await _database.StringSetAsync(basket.BuyerId, JsonSerializer.Serialize(basket, JsonDefaults.CaseInsensitiveOptions));
        if (!created)
        {
            _logger.LogInformation("Problem occur persisting the item");
            return null;
        }
        
        _logger.LogInformation("Basket item persisted successfully");

        return await GetBasketAsync(basket.BuyerId);
    }

    public async Task<bool> DeleteBasketAsync(string id)
    {
        return await _database.KeyDeleteAsync(id);
    }

    private IServer GetServer()
    {
        var endpoint = _redis.GetEndPoints();
        return _redis.GetServer(endpoint.First());
    }
}