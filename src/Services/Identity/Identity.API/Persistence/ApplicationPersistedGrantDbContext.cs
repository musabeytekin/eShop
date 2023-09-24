using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Persistence
{
    public class ApplicationPersistedGrantDbContext : PersistedGrantDbContext
    {
        public ApplicationPersistedGrantDbContext(DbContextOptions<PersistedGrantDbContext> options,
            OperationalStoreOptions storeOptions) : base(options, storeOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}