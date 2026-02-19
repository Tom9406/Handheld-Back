using Microsoft.EntityFrameworkCore;
using Wms.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (SQL Server)
builder.Services.AddDbContext<WmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WmsDb")));





var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();





app.Run();
