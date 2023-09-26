using Catalog.API.Extensions;
using CatalogApi;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Catalog.API.Grpc;

using static CatalogApi.Catalog;

public class CatalogService : CatalogBase
{
    private readonly CatalogDbContext _catalogDbContext;
    private readonly CatalogSettings _settings;
    private readonly ILogger _logger;

    public CatalogService(CatalogDbContext dbContext, IOptions<CatalogSettings> settings,
        ILogger<CatalogService> logger)
    {
        _settings = settings.Value;
        _catalogDbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger;
    }

    public override async Task<CatalogItemResponse> GetItemById(CatalogItemRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Begin grpc call from method CatalogService.GetItemById for request {id}", request.Id);

        if (request.Id <= 0)
        {
            context.Status = new Status(StatusCode.FailedPrecondition, "Id must be positive, received: " + request.Id);
            return null;
        }

        var item = await _catalogDbContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == request.Id);
        var baseUri = _settings.PicBaseUrl;
        item.FillProductUrl(baseUri);

        if (item != null)
        {
            return new CatalogItemResponse()
            {
                AvailableStock = item.AvailableStock,
                Description = item.Description,
                Id = item.Id,
                MaxStockThreshold = item.MaxStockThreshold,
                Name = item.Name,
                OnReorder = item.OnReorder,
                PictureFileName = item.PictureFileName,
                PictureUri = item.PictureUri,
                Price = (double)item.Price,
                RestockThreshold = item.RestockThreshold
            };
        }

        context.Status = new Status(StatusCode.NotFound, $"Item with id {request.Id} not found");
        return null;
    }

    public override async Task<PaginatedItemsResponse> GetItemsByIds(CatalogItemsRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Begin grpc call from method CatalogService.GetItemsByIds for request {ids}",
            request.Ids);

        if (!string.IsNullOrEmpty(request.Ids))
        {
            var items = await GetItemsByIdsAsync(request.Ids);

            context.Status = !items.Any()
                ? new Status(StatusCode.NotFound, $"ids value invalid. Must be comma-separated list of numbers")
                : new Status(StatusCode.OK, string.Empty);

            // convert the CatalogItem to CatalogItemResponse
            return this.MapToResponse(items);
        }
        
        var totalItems = await _catalogDbContext.CatalogItems.LongCountAsync();
        var itemsOnPage = await _catalogDbContext.CatalogItems
            .OrderBy(c => c.Name)
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
        
        var model = this.MapToResponse(itemsOnPage, totalItems, request.PageIndex, request.PageSize);
        context.Status = new Status(StatusCode.OK, string.Empty);
        
        return model;
    }

    private async Task<List<CatalogItem>> GetItemsByIdsAsync(string ids)
    {
        var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));
        if (!numIds.All(nid => nid.Ok))
        {
            return new List<CatalogItem>();
        }

        var idsToSelect = numIds.Select(id => id.Value);

        var items = await _catalogDbContext.CatalogItems.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();

        items = ChangeUriPlaceholder(items);

        return items;
    }

    private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
    {
        var baseUri = _settings.PicBaseUrl;
        items.ForEach(x => x.FillProductUrl(baseUri));
        return items;
    }

    private PaginatedItemsResponse MapToResponse(List<CatalogItem> items, long count, int pageIndex, int pageSize)
    {
        var result = new PaginatedItemsResponse()
        {
            Count = count,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        items.ForEach(item =>
        {
            var brand = item.CatalogBrand == null
                ? null
                : new CatalogApi.CatalogBrand()
                {
                    Id = item.CatalogBrand.Id,
                    Name = item.CatalogBrand.Brand
                };

            var catalogType = item.CatalogType == null
                ? null
                : new CatalogApi.CatalogType()
                {
                    Id = item.CatalogType.Id,
                    Type = item.CatalogType.Type
                };

            result.Data.Add(new CatalogItemResponse()
            {
                AvailableStock = item.AvailableStock,
                Description = item.Description,
                Id = item.Id,
                MaxStockThreshold = item.MaxStockThreshold,
                Name = item.Name,
                OnReorder = item.OnReorder,
                PictureFileName = item.PictureFileName,
                PictureUri = item.PictureUri,
                RestockThreshold = item.RestockThreshold,
                CatalogBrand = brand,
                CatalogType = catalogType,
                Price = (double)item.Price
            });
        });

        return result;
    }

    private PaginatedItemsResponse MapToResponse(List<CatalogItem> items)
    {
        return this.MapToResponse(items, items.Count, 1, items.Count);
    }
}