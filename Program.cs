using TraCuuBHXH_BHYT.Data;
using Microsoft.EntityFrameworkCore;
using TraCuuBHXH_BHYT.Service;
using TraCuuBHXH_BHYT.Helpers;
using TraCuuBHXH_BHYT.Interface;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var encodedConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = ConnectionStringHelper.DecodeBase64(encodedConnectionString);

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
