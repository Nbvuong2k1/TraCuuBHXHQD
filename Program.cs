using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Serilog;
using TraCuuBHXH_BHYT.Data;
using TraCuuBHXH_BHYT.Helpers;
using TraCuuBHXH_BHYT.Interface;
using TraCuuBHXH_BHYT.Service;
var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/startup-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();


var logger = builder.Logging.Services.BuildServiceProvider()
    .GetRequiredService<ILogger<Program>>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//var connectionString = ConnectionStringHelper.DecodeBase64(encodedConnectionString);
//Mở Command Prompt / PowerShell (Run as Administrator)
//setx BHXH_SECRET_KEY "key-rieng"
string secretKey = Environment.GetEnvironmentVariable("BHXH_TraCuu");
if (string.IsNullOrWhiteSpace(secretKey))
{
    logger.LogCritical("❌ Missing BHXH_SECRET_KEY environment variable");
    //throw new InvalidOperationException("Missing BHXH_SECRET_KEY");
}
else
{
    connectionString = ConnectionStringCrypto.Decrypt(connectionString, secretKey).Trim()
        .Replace("\0", "");
    connectionString = connectionString.Replace(@"\\", @"\");
    var sqlBuilder = new SqlConnectionStringBuilder(connectionString);
    connectionString= sqlBuilder.ConnectionString;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<ITraCuuBHXHService, TraCuuBHXHService>();
builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();

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
