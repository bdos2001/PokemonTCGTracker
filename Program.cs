using PokemonTCGTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IPokemonApiService, PokemonApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.tcgdex.net/v2/en/");
});

builder.Services.AddSingleton<PokemonTCGTracker.Services.WatchlistService>();

builder.Services.AddSingleton<PokemonTCGTracker.Services.PortfolioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
