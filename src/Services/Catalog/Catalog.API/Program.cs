var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddGrpc();
builder.Services.AddControllers();

builder.Services.AddDbContexts(builder.Configuration);
builder.Services.AddApplicationOptions(builder.Configuration);
builder.Services.AddHealthChecks(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultServices();
app.MapControllers();
app.MapPicApi();

app.MapGrpcService<CatalogService>();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    var settings = scope.ServiceProvider.GetRequiredService<IOptions<CatalogSettings>>();
    var logger = app.Services.GetService<ILogger<CatalogDbContextSeed>>();

    await context.Database.MigrateAsync();
    await new CatalogDbContextSeed().SeedAsync(context, app.Environment, settings, logger);
}

await app.RunAsync();