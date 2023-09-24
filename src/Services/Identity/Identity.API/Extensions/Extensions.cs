using Identity.API.Models;
using Identity.API.Persistence;
using Identity.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API.Extensions
{
    public static class Extensions
    {
        public static IServiceCollection AddIdentityServerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
                })
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();


            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    options.EmitStaticAudienceClaim = true;
                })
                .AddDeveloperSigningCredential()
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                    options.EnableTokenCleanup = true;
                })
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<ProfileService>();
            
            builder.AddResourceOwnerValidator<IdentityResourceOwnerPasswordValidator>();
            
            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
            return services;
        }
        
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationIdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<ApplicationPersistedGrantDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<ApplicationConfigurationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            return services;
        }
    }
}