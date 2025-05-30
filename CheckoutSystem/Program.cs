using CheckoutSystem.Repositories;
using CheckoutSystem.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add memory cache
builder.Services.AddMemoryCache();

// Register repositories
builder.Services.AddSingleton<BasketRepository>();
builder.Services.AddSingleton<IBasketRepository>(provider =>
{
    var basketRepo = provider.GetRequiredService<BasketRepository>();
    var memoryCache = provider.GetRequiredService<IMemoryCache>();
    var logger = provider.GetRequiredService<ILogger<CachedBasketRepository>>();

    return new CachedBasketRepository(basketRepo, memoryCache, logger);
});

// Register checkout service
builder.Services.AddSingleton<CheckoutService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
