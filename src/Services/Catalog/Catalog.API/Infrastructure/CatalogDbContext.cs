namespace Catalog.API.Infrastructure;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<CatalogItem> CatalogItems { get; set; }
    public DbSet<CatalogBrand> CatalogBrands { get; set; }
    public DbSet<CatalogType> CatalogTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
        builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
        builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());
    }
    
    // public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    // {
    //     public CatalogDbContext CreateDbContext(string[] args)
    //     {
    //         var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>()
    //             .UseSqlServer("Server=.;Initial Catalog=;Integrated Security=true");
    //
    //         return new CatalogContext(optionsBuilder.Options);
    //     }
    // }
}