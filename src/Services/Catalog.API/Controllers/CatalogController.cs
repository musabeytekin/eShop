namespace Catalog.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class CatalogController : ControllerBase
{
    private readonly CatalogDbContext _catalogDbContext;
    private readonly CatalogSettings _settings;

    public CatalogController(CatalogDbContext context, IOptionsSnapshot<CatalogSettings> settings)
    {
        _catalogDbContext = context;
        _settings = settings.Value;

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    // GET api/v1/[controller]/items[?pageSize=3&pageIndex=10]
    [HttpGet]
    [Route("items")]
    public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0,
        string ids = null)
    {
        if (!string.IsNullOrEmpty(ids))
        {
            var items = await GetItemsByIdsAsync(ids);
            if (!items.Any())
            {
                return BadRequest("ids value invalid. Must be comma-separated list of numbers");
            }

            return Ok(items);
        }

        var totalItems = await _catalogDbContext.CatalogItems
            .LongCountAsync();

        var itemsOnPage = await _catalogDbContext.CatalogItems
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

        return Ok(model);
    }

    private async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
    {
        // convert the comma-separated list of ids into an integer list
        var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));

        // if any of the ids fail to parse, return an empty list
        if (!numIds.All(nid => nid.Ok))
        {
            return new List<CatalogItem>();
        }

        // convert the list of ids into a list of integers
        var idsToSelect = numIds.Select(id => id.Value);

        // select the catalog items where the id is in the list of integers
        var items = await _catalogDbContext.CatalogItems.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();

        // change the uri placeholder to the actual uri
        items = ChangeUriPlaceholder(items);

        return items;
    }


    private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
    {
        var baseUri = _settings.PicBaseUrl;
        items.ForEach(x => x.FillProductUrl(baseUri));
        return items;
    }
}