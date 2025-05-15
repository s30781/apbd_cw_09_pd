using Tutorial9.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IWarehouseService, WarehouseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

//np POST http://localhost:5027/api/warehouse
//{
//"IdProduct": 1,
//"IdWarehouse": 2,
//"Amount": 5,
//"CreatedAt": "2024-05-15T12:00:00"
//}

