using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Services.Common
{
    public static class CommonExtensions
    {
        // public static IServiceCollection AddDefaultServices(this IServiceCollection services,
        //     IConfiguration configuration)
        // {
        //     services.AddDefaultAuthentication(configuration);
        //     services.AddHttpContextAccessor();
        //     return services;
        // }

        public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
        {
            builder.Services.AddDefaultAuthentication(builder.Configuration);
            builder.Services.AddHttpContextAccessor();
            return builder;
        }

        public static IServiceCollection AddDefaultAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {
            var identitySection = configuration.GetSection("Identity");

            if (!identitySection.Exists())
            {
                //if no identity section is found, return the services, no authentication is needed
                return services;
            }

            // prevent from mapping "sub" claim to nameidentifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            // add authentication services
            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    var identityUrl = identitySection.GetRequiredValue("Url");
                    var audience = identitySection.GetRequiredValue("Audience");

                    options.Authority = identityUrl;
                    options.RequireHttpsMetadata = false;
                    options.Audience = audience;
                    options.TokenValidationParameters.ValidateAudience = false; //???
                });
            return services;
        }

        public static WebApplication UseDefaultServices(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseRouting();

            var identitySection = app.Configuration.GetSection("Identity");

            if (identitySection.Exists())
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            return app;
        }
    }
}