using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Persistence
{
    public class ApplicationConfigurationDbContext : ConfigurationDbContext
    {
        public ApplicationConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options,
            ConfigurationStoreOptions storeOptions) : base(options, storeOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}