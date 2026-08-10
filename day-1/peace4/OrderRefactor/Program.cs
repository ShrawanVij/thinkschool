using Microsoft.EntityFrameworkCore;
using OrderRefactor.Data;
using OrderRefactor.Repositories;
using OrderRefactor.Services;
using OrderRefactor.Pricing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPricingStrategy, LargeOrderDiscountStrategy>();
builder.Services.AddScoped<IPricingStrategy, VipDiscountStrategy>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program
{
}