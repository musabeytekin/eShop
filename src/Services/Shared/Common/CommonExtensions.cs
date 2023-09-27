using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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
            builder.Services.AddDefaultOpenApi(builder.Configuration);
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

            app.UseDefaultOpenApi(app.Configuration);
            return app;
        }

        public static IServiceCollection AddDefaultOpenApi(this IServiceCollection services,
            IConfiguration configuration)
        {
            var openApiSection = configuration.GetSection("OpenApi");
            if (!(openApiSection).Exists())
            {
                return services;
            }

            services.AddEndpointsApiExplorer();

            return services.AddSwaggerGen(options =>
            {
                var document = openApiSection.GetRequiredSection("Document");
                var version = document.GetRequiredValue("Version") ?? "v1";
                options.SwaggerDoc(version, new OpenApiInfo()
                {
                    Title = document.GetRequiredValue("Title"),
                    Version = version,
                    Description = document.GetRequiredValue("Description")
                });

                // TODO: Add support for security
            });
        }

        public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app, IConfiguration configuration)
        {
            var openApiSection = configuration.GetSection("OpenApi");

            if (!openApiSection.Exists())
            {
                return app;
            }

            app.UseSwagger();
            app.UseSwaggerUI(setup =>
            {
                var endpoint = openApiSection.GetRequiredSection("Endpoint");
                var url = endpoint.GetValue<string>("Url");

                var swaggerUrl = url ?? "/swagger/v1/swagger.json";

                setup.SwaggerEndpoint(swaggerUrl, endpoint.GetRequiredValue("Name"));
            });

            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

            return app;
        }
    }
}