using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderRefactor.Data;
using OrderRefactor.Models;

namespace OrderRefactor.Tests;

public class OrderApiIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderApiIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        db.Customers.Add(new Customer
        {
            Id = 1,
            Name = "Test Customer",
            Email = "test@example.com",
            IsVip = false
        });

        db.Products.Add(new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 100,
            Stock = 10
        });

        db.SaveChanges();

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostOrder_ReturnsCreated()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 1
                }
            ]
        };

        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            request);

        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }
}