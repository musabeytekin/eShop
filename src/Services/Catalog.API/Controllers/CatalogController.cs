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

    // GET api/v1/[controller]/items/{id}

    [HttpGet]
    [Route("items/{id:int}")]
    public async Task<IActionResult> ItemByIdAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var item = await _catalogDbContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);
        var baseUri = _settings.PicBaseUrl;
        
        item.FillProductUrl(baseUri);

        if (item != null)
            return Ok(item);

        return NotFound();
    }
    
    // GET api/v1/[controller]/items/withname/{name}
    [HttpGet]
    [Route("items/withname/{name:minlength(1)}")]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsWithNameAsync(string name,
        [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var totalItems = await _catalogDbContext.CatalogItems
            .Where(c => c.Name.StartsWith(name))
            .LongCountAsync();

        var itemsOnPage = await _catalogDbContext.CatalogItems
            .Where(c => c.Name.StartsWith(name))
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);

        return Ok(model);
    }
    
    // GET api/v1/[controller]/items/type/1/brand[?pageSize=3&pageIndex=10]
    [HttpGet]
    [Route("items/type/{catalogTypeId}/brand/{catalogBrandId:int?}")]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByTypeIdAndBrandIdAsync(int catalogTypeId, int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        // IQueryable to hold the root query, because the query needs to be built dynamically based on the presence of catalogBrandId
        // root = SELECT * FROM CatalogItems
        var root = (IQueryable<CatalogItem>)_catalogDbContext.CatalogItems;

        // root = Select * FROM CatalogItems WHERE CatalogTypeId = catalogTypeId
        root = root.Where(ci => ci.CatalogTypeId == catalogTypeId);

        // if catalogBrandId has a value, then add the where clause to the root query
        // root = Select * FROM CatalogItems WHERE CatalogTypeId = catalogTypeId AND CatalogBrandId = catalogBrandId
        if (catalogBrandId.HasValue)
        {
            root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
        }

        // totalItems = SELECT COUNT(*) FROM root
        var totalItems = await root
            .LongCountAsync();

        // now the root query sends to the database and returns the results
        var itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }
    
    // GET api/v1/[controller]/items/type/all/brand[?pageSize=3&pageIndex=10]
    [HttpGet]
    [Route("items/type/all/brand/{catalogBrandId:int?}")]
    public async Task<ActionResult<PaginatedItemsViewModel<CatalogItem>>> ItemsByBrandIdAsync(int? catalogBrandId, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
    {
        var root = (IQueryable<CatalogItem>)_catalogDbContext.CatalogItems;

        if (catalogBrandId.HasValue)
        {
            root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
        }

        var totalItems = await root
            .LongCountAsync();

        var itemsOnPage = await root
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

        return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
    }
    
    // GET api/v1/[controller]/CatalogTypes
    [HttpGet]
    [Route("catalogtypes")]
    public async Task<ActionResult<List<CatalogType>>> CatalogTypesAsync()
    {
        return await _catalogDbContext.CatalogTypes.ToListAsync();
    }
    
    // GET api/v1/[controller]/CatalogBrands
    [HttpGet]
    [Route("catalogbrands")]
    public async Task<ActionResult<List<CatalogBrand>>> CatalogBrandsAsync()
    {
        return await _catalogDbContext.CatalogBrands.ToListAsync();
    }
    


    private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
    {
        var baseUri = _settings.PicBaseUrl;
        items.ForEach(x => x.FillProductUrl(baseUri));
        return items;
    }
}