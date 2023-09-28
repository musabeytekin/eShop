using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddRedis(builder.Configuration);

builder.Services.AddTransient<IBasketRepository, RedisBasketRepository>();
builder.Services.AddTransient<IIdentityService, IdentityService>();

var app = builder.Build();


app.UseDefaultServices();
app.MapControllers();

await app.RunAsync();