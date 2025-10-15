using Microsoft.EntityFrameworkCore;
using WeatherAppAPI.Data;
using WeatherAppAPI.Services;
using Serilog;
using ILogger = Serilog.ILogger;
using WeatherAppAPI.RateLimits;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddHttpClient("Weather", config =>
{
    config.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");
});

builder.Logging.ClearProviders();
ILogger logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Logging.AddSerilog(logger);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<WeatherDbContext>(
        options => options.UseSqlServer(connectionString));

var redisConnection = builder.Configuration.GetValue<string>("RedisConfig:ConnectionString") 
    ?? throw new InvalidOperationException("Connection string 'RedisConfig' not found."); ;
var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<RedisUserRateLimiter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
