using System;
using System.Linq;
using System.Threading.Tasks;
using Identity.API.Configuration;
using Identity.API.Models;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Serilog;

namespace Identity.API.Persistence
{
    public class SeedData
    {
        public async static Task EnsureSeedData(IServiceScope scope, IConfiguration configuration, ILogger logger)
        {
            var retryPolicy = CreateRetryPolicy(configuration, logger);
            var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
            var persistedGrantDbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationPersistedGrantDbContext>();
            var configurationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationConfigurationDbContext>();

            await retryPolicy.ExecuteAsync(async () =>
            {
                await identityDbContext.Database.MigrateAsync();
                await persistedGrantDbContext.Database.MigrateAsync();
                await configurationDbContext.Database.MigrateAsync();

                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var joe = await userMgr.FindByNameAsync("joe");

                if (joe == null)
                {
                    joe = new ApplicationUser
                    {
                        UserName = "joe",
                        Email = "joeTribbiani@email.com",
                        EmailConfirmed = true,
                        CardHolderName = "Joe Tribbiani",
                        CardNumber = "4012888888881881",
                        CVV = "410",
                        City = "Istanbul",
                        Country = "TR",
                        Expiration = "08/29",
                        Id = Guid.NewGuid().ToString(),
                        LastName = "Tribbiani",
                        Name = "Joe",
                        PhoneNumber = "1234567890",
                        ZipCode = "06080",
                        Street = "17203 TG 61st Ct",
                    };

                    var result = await userMgr.CreateAsync(joe, "Password12*");

                    if (!result.Succeeded)
                    {
                        logger.Information("joe creation failed, already exists?");
                        throw new Exception(result.Errors.First().Description);
                    }

                    logger.Debug("joe created");
                }
                else
                {
                    logger.Debug("joe already exists");
                }

                if (!(await configurationDbContext.Clients.AnyAsync()))
                {
                    configurationDbContext.Clients.AddRange(Config.Clients.Select(client => client.ToEntity()));
                    await configurationDbContext.SaveChangesAsync();
                }

                if (!(await configurationDbContext.ApiResources.AnyAsync()))
                {
                    configurationDbContext.ApiResources.AddRange(Config.Apis.Select(api => api.ToEntity()));
                    await configurationDbContext.SaveChangesAsync();
                }

                if (!(await configurationDbContext.ApiScopes.AnyAsync()))
                {
                    configurationDbContext.ApiScopes.AddRange(Config.ApiScopes.Select(api => api.ToEntity()));
                    await configurationDbContext.SaveChangesAsync();
                }

                if (!(await configurationDbContext.IdentityResources.AnyAsync()))
                {
                    configurationDbContext.IdentityResources.AddRange(
                        Config.IdentityResources.Select(api => api.ToEntity()));
                    await configurationDbContext.SaveChangesAsync();
                }

            });
        }

        private static AsyncPolicy CreateRetryPolicy(IConfiguration configuration, ILogger logger)
        {
            var retryMigrations = false;

            bool.TryParse(configuration["RetryMigrations"], out retryMigrations);

            if (retryMigrations)
            {
                return Policy.Handle<Exception>().WaitAndRetryForeverAsync(
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, retry, timeSpan) => logger.Warning(exception,
                        "Error migrating database (retry attempt {retry})", retry));
            }

            return Policy.NoOpAsync();
        }

  
    }
}