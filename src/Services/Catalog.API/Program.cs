using Catalog.API;
using Catalog.API.Extensions;
using Services.Common;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddDbContexts(builder.Configuration);
builder.Services.AddApplicationOptions(builder.Configuration);
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultServices();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    var settings = scope.ServiceProvider.GetRequiredService<IOptions<CatalogSettings>>();
    var logger = app.Services.GetService<ILogger<CatalogDbContextSeed>>();

    await context.Database.MigrateAsync();
    await new CatalogDbContextSeed().SeedAsync(context, app.Environment, settings, logger);
}

await app.RunAsync();