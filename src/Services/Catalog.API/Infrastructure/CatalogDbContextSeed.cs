

namespace Catalog.API.Infrastructure;

public class CatalogDbContextSeed
{
    public async Task SeedAsync(CatalogDbContext context, IWebHostEnvironment env, IOptions<CatalogSettings> settings,
        ILogger<CatalogDbContextSeed> logger)
    {
        var policy = CreatePolicy(logger, nameof(CatalogDbContextSeed));

        await policy.ExecuteAsync(async () =>
        {
            var useCustomizationData = settings.Value.UseCustomizationData;
            var contentRootPath = env.ContentRootPath;
            var picturePath = env.WebRootPath;

            if (!context.CatalogBrands.Any())
            {
                // if brand data is not in db, load it from the file
                // if use custom data flag is true then use the custom brand data
                await context.CatalogBrands.AddRangeAsync(useCustomizationData
                    ? GetPreconfiguredCatalogBrands()
                    : GetCatalogBrandsFromFile(contentRootPath, logger));
                await context.SaveChangesAsync();
            }

            if (!context.CatalogTypes.Any())
            {
                // if type data is not in db, load it from the file
                // if use custom data flag is true then use the custom type data
                await context.CatalogTypes.AddRangeAsync(useCustomizationData
                    ? GetPreconfiguredCatalogTypes()
                    : GetCatalogTypesFromFile(contentRootPath, logger));
                await context.SaveChangesAsync();
            }

            if (!context.CatalogItems.Any())
            {
                // if item data is not in db, load it from the file
                // if use custom data flag is true then use the custom item data
                await context.CatalogItems.AddRangeAsync(useCustomizationData
                    ? GetPreconfiguredItems()
                    : GetCatalogItemsFromFile(contentRootPath, context, logger));
                await context.SaveChangesAsync();

                // GetCatalogItemPictures(contentRootPath, picturePath, logger);
            }
        });
    }


    // Catalog brand 
    private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentRootPath,
        ILogger<CatalogDbContextSeed> logger)
    {
        // get the path of the Catalog brands csv file
        // example path: C:\Users\username\source\repos\microservices-dotnetcore\src\Services\Catalog\Catalog.API\Setup\CatalogBrands.csv
        string csvFileCatalogBrands = Path.Combine(contentRootPath, "Setup", "CatalogBrands.csv");

        // if the file does not exist, return the preconfigured catalog brands
        if (!File.Exists(csvFileCatalogBrands))
            return GetPreconfiguredCatalogBrands();

        // for reading csv headers
        string[] csvHeaders;

        try
        {
            // get the headers of the csv file
            string[] requiredHeaders = { "catalogbrand" };
            csvHeaders = GetHeaders(csvFileCatalogBrands, requiredHeaders);
        }
        catch (Exception ex)
        {
            // if the headers are not correct, return the preconfigured catalog brands
            logger.LogError(ex.Message);
            return GetPreconfiguredCatalogBrands();
        }

        return File.ReadAllLines(csvFileCatalogBrands)
            .Skip(1) // skip header row
            .SelectTry(CreateCatalogBrand) // = SelectTry<string, CatalogBrand>(CreateCatalogBrand)
            .OnCaughtException(ex =>
            {
                logger.LogError(ex, "Error creating brand while seeding database");
                return null;
            })
            .Where(x => x != null);
    }

    private CatalogBrand CreateCatalogBrand(string brand)
    {
        // trim whitespace and quotes
        brand = brand.Trim('"').Trim();


        // if the brand is empty, throw an exception
        if (string.IsNullOrEmpty(brand))
        {
            throw new Exception("Catalog Brand Name is empty");
        }

        // return the catalog brand
        return new CatalogBrand
        {
            Brand = brand,
        };
    }

    private IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands()
    {
        return new List<CatalogBrand>()
        {
            new() { Brand = "Azure" },
            new() { Brand = ".NET" },
            new() { Brand = "Visual Studio" },
            new() { Brand = "SQL Server" },
            new() { Brand = "Other" }
        };
    }

    // Catalog type
    private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentRootPath,
        ILogger<CatalogDbContextSeed> logger)
    {
        // get the path of the Catalog types csv file
        // example path: C:\Users\username\source\repos\microservices-dotnetcore\src\Services\Catalog\Catalog.API\Setup\CatalogTypes.csv

        string csvFileCatalogTypes = Path.Combine(contentRootPath, "Setup", "CatalogTypes.csv");

        if (!File.Exists(csvFileCatalogTypes))
            return GetPreconfiguredCatalogTypes();

        string[] csvHeaders;
        try
        {
            string[] requiredHeaders = { "catalogtype" };
            csvHeaders = GetHeaders(csvFileCatalogTypes, requiredHeaders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading CSV headers");
            return GetPreconfiguredCatalogTypes();
        }

        return File.ReadAllLines(csvFileCatalogTypes)
            .Skip(1) // skip header row
            .SelectTry(CreateCatalogType)
            .OnCaughtException(ex =>
            {
                logger.LogError(ex, "Error creating catalog type while seeding database");
                return null;
            })
            .Where(x => x != null);
    }

    private CatalogType CreateCatalogType(string type)
    {
        type = type.Trim('"').Trim();

        if (string.IsNullOrEmpty(type))
        {
            throw new Exception("Catalog Type Name is empty");
        }

        return new CatalogType
        {
            Type = type,
        };
    }

    private IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
    {
        return new List<CatalogType>()
        {
            new() { Type = "Mug" },
            new() { Type = "T-Shirt" },
            new() { Type = "Sheet" },
            new() { Type = "USB Memory Stick" }
        };
    }

    // Catalog item

    private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentRootPath, CatalogDbContext context,
        ILogger<CatalogDbContextSeed> logger)
    {
        // get the path of the Catalog items csv file
        // example path: C:\Users\username\source\repos\microservices-dotnetcore\src\Services\Catalog\Catalog.API\Setup\CatalogItems.csv
        string csvFileCatalogItems = Path.Combine(contentRootPath, "Setup", "CatalogItems.csv");

        if (!File.Exists(csvFileCatalogItems))
            return GetPreconfiguredItems();

        string[] csvHeaders;
        try
        {
            string[] requiredHeaders =
            {
                "catalogtypename", "catalogbrandname", "description", "name", "price", "picturefilename"
            };
            string[] optionalHeaders = { "availablestock", "restockthreshold", "maxstockthreshold", "onreorder" };
            csvHeaders = GetHeaders(csvFileCatalogItems, requiredHeaders, optionalHeaders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading CSV headers");
            return GetPreconfiguredItems();
        }

        var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
        var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(ct => ct.Brand, ct => ct.Id);

        return File.ReadAllLines(csvFileCatalogItems)
            .Skip(1) // skip header row
            .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
            .SelectTry(column => CreateCatalogItem(column, csvHeaders, catalogTypeIdLookup, catalogBrandIdLookup))
            .OnCaughtException(ex =>
            {
                logger.LogError(ex, "Error creating catalog item while seeding database");
                return null;
            })
            .Where(x => x != null);
    }

    private CatalogItem CreateCatalogItem(string[] column, string[] headers,
        Dictionary<string, int> catalogTypeIdLookup, Dictionary<string, int> catalogBrandIdLookup)
    {
        // if the column length is not the same as the headers length, throw an exception
        if (column.Count() != headers.Count())
        {
            throw new Exception($"column count '{column.Count()}' not the same as headers count'{headers.Count()}'");
        }

        // get catalog type name from column and trim whitespace and quotes
        string catalogTypeName = column[Array.IndexOf(headers, "catalogtypename")].Trim('"').Trim();

        // if catalogTypeName is not in the catalogTypeIdLookup, throw an exception
        if (!catalogTypeIdLookup.ContainsKey(catalogTypeName))
        {
            throw new Exception($"({catalogTypeName}) does not exist in catalog types");
        }

        // get catalog brand name from column and trim whitespace and quotes
        string catalogBrandName = column[Array.IndexOf(headers, "catalogbrandname")].Trim('"').Trim();

        // if catalogBrandName is not in the catalogBrandIdLookup, throw an exception
        if (!catalogBrandIdLookup.ContainsKey(catalogBrandName))
        {
            throw new Exception($"({catalogBrandName}) does not exist in catalog brands");
        }

        //check if the price is valid, if invalid throw an exception
        string priceString = column[Array.IndexOf(headers, "price")].Trim('"').Trim();
        if (!decimal.TryParse(priceString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                out decimal price))
        {
            throw new Exception($"price={priceString} is not a valid decimal number");
        }

        // create the catalog item with required datas
        var catalogItem = new CatalogItem()
        {
            CatalogTypeId = catalogTypeIdLookup[catalogTypeName],
            CatalogBrandId = catalogBrandIdLookup[catalogBrandName],
            Description = column[Array.IndexOf(headers, "description")].Trim('"').Trim(),
            Name = column[Array.IndexOf(headers, "name")].Trim('"').Trim(),
            Price = price,
            PictureFileName = column[Array.IndexOf(headers, "picturefilename")].Trim('"').Trim(),
        };

        // check available stock is not empty, if it is empty throw an exception, if it is not empty fill the catalogItem available stock
        int availableStockIndex = Array.IndexOf(headers, "availablestock");
        if (availableStockIndex != -1)
        {
            string availableStockString = column[availableStockIndex].Trim('"').Trim();
            if (!string.IsNullOrEmpty(availableStockString))
            {
                if (!int.TryParse(availableStockString, out int availableStock))
                {
                    throw new Exception($"available stock={availableStockString} is not a valid integer number");
                }

                catalogItem.AvailableStock = availableStock;
            }
        }
        
        // check restock threshold is not empty, if it is empty throw an exception, if it is not empty fill the catalogItemRrestockThreshold
        int restockThresholdIndex = Array.IndexOf(headers, "restockthreshold");
        if (restockThresholdIndex != -1)
        {
            string restockThresholdString = column[restockThresholdIndex].Trim('"').Trim();
            if (!string.IsNullOrEmpty(restockThresholdString))
            {
                if (int.TryParse(restockThresholdString, out int restockThreshold))
                {
                    catalogItem.RestockThreshold = restockThreshold;
                }
                else
                {
                    throw new Exception($"restockThreshold={restockThreshold} is not a valid integer");
                }
            }
        }
        
        // check max stock threshold is not empty, if it is empty throw an exception, if it is not empty fill the catalogItem MaxStockThreshold
        int maxStockThresholdIndex = Array.IndexOf(headers, "maxstockthreshold");
        if (maxStockThresholdIndex != -1)
        {
            string maxStockThresholdString = column[maxStockThresholdIndex].Trim('"').Trim();
            if (!string.IsNullOrEmpty(maxStockThresholdString))
            {
                if (int.TryParse(maxStockThresholdString, out int maxStockThreshold))
                {
                    catalogItem.MaxStockThreshold = maxStockThreshold;
                }
                else
                {
                    throw new Exception($"maxStockThreshold={maxStockThreshold} is not a valid integer");
                }
            }
        }

        // check on reorder is not empty, if it is empty throw an exception, if it is not empty fill the catalogItem OnReorder
        int onReorderIndex = Array.IndexOf(headers, "onreorder");
        if (onReorderIndex != -1)
        {
            string onReorderString = column[onReorderIndex].Trim('"').Trim();
            if (!string.IsNullOrEmpty(onReorderString))
            {
                if (bool.TryParse(onReorderString, out bool onReorder))
                {
                    catalogItem.OnReorder = onReorder;
                }
                else
                {
                    throw new Exception($"onReorder={onReorderString} is not a valid boolean");
                }
            }
        }

        return catalogItem;
        
        
    }

    private IEnumerable<CatalogItem> GetPreconfiguredItems()
    {
        return new List<CatalogItem>()
        {
            new()
            {
                CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Bot Black Hoodie",
                Name = ".NET Bot Black Hoodie", Price = 19.5M, PictureFileName = "1.png"
            },
            new()
            {
                CatalogTypeId = 1, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Black & White Mug",
                Name = ".NET Black & White Mug", Price = 8.50M, PictureFileName = "2.png"
            },
            new()
            {
                CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = "Prism White T-Shirt",
                Name = "Prism White T-Shirt", Price = 12, PictureFileName = "3.png"
            },
            new()
            {
                CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Foundation T-shirt",
                Name = ".NET Foundation T-shirt", Price = 12, PictureFileName = "4.png"
            },
            new()
            {
                CatalogTypeId = 3, CatalogBrandId = 5, AvailableStock = 100, Description = "Roslyn Red Sheet",
                Name = "Roslyn Red Sheet", Price = 8.5M, PictureFileName = "5.png"
            },
            new()
            {
                CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Blue Hoodie",
                Name = ".NET Blue Hoodie", Price = 12, PictureFileName = "6.png"
            },
            new()
            {
                CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = "Roslyn Red T-Shirt",
                Name = "Roslyn Red T-Shirt", Price = 12, PictureFileName = "7.png"
            },
            new()
            {
                CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = "Kudu Purple Hoodie",
                Name = "Kudu Purple Hoodie", Price = 8.5M, PictureFileName = "8.png"
            },
            new()
            {
                CatalogTypeId = 1, CatalogBrandId = 5, AvailableStock = 100, Description = "Cup<T> White Mug",
                Name = "Cup<T> White Mug", Price = 12, PictureFileName = "9.png"
            },
            new()
            {
                CatalogTypeId = 3, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Foundation Sheet",
                Name = ".NET Foundation Sheet", Price = 12, PictureFileName = "10.png"
            },
            new()
            {
                CatalogTypeId = 3, CatalogBrandId = 2, AvailableStock = 100, Description = "Cup<T> Sheet",
                Name = "Cup<T> Sheet", Price = 8.5M, PictureFileName = "11.png"
            },
            new()
            {
                CatalogTypeId = 2, CatalogBrandId = 5, AvailableStock = 100, Description = "Prism White TShirt",
                Name = "Prism White TShirt", Price = 12, PictureFileName = "12.png"
            },
        };
    }

    private string[] GetHeaders(string csvfile, string[] requiredHeaders, string[] optionalHeaders = null)
    {
        string[] csvheaders = File.ReadLines(csvfile).First().ToLowerInvariant().Split(',');

        if (csvheaders.Count() < requiredHeaders.Count())
        {
            throw new Exception(
                $"requiredHeader count '{requiredHeaders.Count()}' is bigger then csv header count '{csvheaders.Count()}' ");
        }

        if (optionalHeaders != null)
        {
            if (csvheaders.Count() > (requiredHeaders.Count() + optionalHeaders.Count()))
            {
                throw new Exception(
                    $"csv header count '{csvheaders.Count()}'  is larger then required '{requiredHeaders.Count()}' and optional '{optionalHeaders.Count()}' headers count");
            }
        }

        foreach (var requiredHeader in requiredHeaders)
        {
            if (!csvheaders.Contains(requiredHeader))
            {
                throw new Exception($"does not contain required header '{requiredHeader}'");
            }
        }

        return csvheaders;
    }


    private AsyncRetryPolicy CreatePolicy(ILogger<CatalogDbContextSeed> logger, string prefix, int retries = 3)
    {
        return Policy.Handle<SqlException>()
            .WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, context) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Error seeding database (attempt {retry} of {retries})",
                        prefix, retry, retries);
                });
    }
}