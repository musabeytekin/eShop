using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Catalog.API.Extensions;

public static class Extensions
{
    public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        //Configure Sql Options 
        static void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlOptions)
        {
            sqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
        }

        //Add Catalog DB Context
        services.AddDbContext<CatalogDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("CatalogDB");
            options.UseSqlServer(connectionString, ConfigureSqlServerOptions);
        });

        return services;
    }

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CatalogSettings>(configuration);

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                // var errors = context.ModelState
                //     .Where(e => e.Value.Errors.Count > 0)
                //     .SelectMany(e => e.Value.Errors)
                //     .Select(e => e.ErrorMessage).ToArray();
                //
                // var errorResponse = new ApiValidationErrorResponse
                // {
                //     Errors = errors
                // };
                //
                // return new BadRequestObjectResult(errorResponse);
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details."
                };

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json", "application/problem+xml" }
                };
                
            };
        });
        return services;
    }

    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();
        hcBuilder.AddSqlServer(_ => configuration.GetRequiredConnectionString("CatalogDB"),
            name: "CatalogDB-check",
            tags: new string[] { "ready" });

        return services;
    }
}