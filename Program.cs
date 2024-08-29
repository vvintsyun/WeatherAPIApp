using Microsoft.EntityFrameworkCore;
using WeatherAppAPI.Data;
using WeatherAppAPI.Services;
using Serilog;
using ILogger = Serilog.ILogger;
using WeatherAppAPI.RateLimits;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddHttpClient("Weather");

builder.Logging.ClearProviders();
ILogger logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Logging.AddSerilog(logger);

builder.Services.AddDbContext<WeatherDbContext>(
        options => options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=WeatherDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"));

builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=WeatherDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
    options.SchemaName = "dbo";
    options.TableName = "UsedRatesCache";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<RateLimitMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
