using System.Text.Json.Serialization;
using Catalog.API.Infrastructure.Exceptions;

namespace Catalog.API.Model;

public class CatalogItem
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public string PictureFileName { get; set; }

    public string PictureUri { get; set; }
    
    public int CatalogTypeId { get; set; }

    [JsonIgnore]
    public CatalogType CatalogType { get; set; }

    public int CatalogBrandId { get; set; }

    [JsonIgnore]
    public CatalogBrand CatalogBrand { get; set; }

    //Current quantity in stock
    public int AvailableStock { get; set; }

    public bool OnReorder { get; set; }

    // Available stock at which we should reorder
    public int RestockThreshold { get; set; }


    // Maximum number of units that can be in-stock at any time (due to physicial/logistical constraints in warehouses)
    public int MaxStockThreshold { get; set; }

    
    public CatalogItem()
    {
    }

    public int RemoveStock(int quantityDesired)
    {
        if (AvailableStock == 0)
        {
            throw new CatalogDomainException($"Empty stock, product item {Name} is sold out");
        }

        if (quantityDesired <= 0)
        {
            throw new CatalogDomainException($"Item units desired should be greater than zero");
        }

        var removed = Math.Min(quantityDesired, this.AvailableStock);

        this.AvailableStock -= removed;

        return removed;
    }
    
    public int AddStock(int quantity)
    {
        var original = this.AvailableStock;

        // The quantity that the client is trying to add to stock is greater than what can be physically accommodated in the Warehouse
        if ((this.AvailableStock + quantity) > this.MaxStockThreshold)
        {
            // For now, this method only adds new units up maximum stock threshold. In an expanded version of this application, we
            //could include tracking for the remaining units and store information about overstock elsewhere. 
            this.AvailableStock += (this.MaxStockThreshold - this.AvailableStock);
        }
        else
        {
            this.AvailableStock += quantity;
        }

        this.OnReorder = false;

        // We return the number of items actually added to stock.
        return this.AvailableStock - original;
    }
}