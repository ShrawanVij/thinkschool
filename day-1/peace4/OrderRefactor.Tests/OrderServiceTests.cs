using Microsoft.Extensions.Logging;
using Moq;
using OrderRefactor.Models;
using OrderRefactor.Repositories;
using OrderRefactor.Services;
using OrderRefactor.Pricing;

namespace OrderRefactor.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repository;
    private readonly Mock<ILogger<OrderService>> _logger;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _repository = new Mock<IOrderRepository>();
        _logger = new Mock<ILogger<OrderService>>();

        var strategies = new List<IPricingStrategy>
        {
            new LargeOrderDiscountStrategy(),
            new VipDiscountStrategy()
        };

        _service = new OrderService(
            _repository.Object,
            _logger.Object,
            strategies);
    }

    [Fact]
    public async Task CreateOrder_InvalidCustomer_ThrowsArgumentException()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 0,
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 1
                }
            ]
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateOrderAsync(
                request,
                CancellationToken.None));
    }
    // Test: validation rejects orders with zero quantity
    [Fact]
    public async Task CreateOrder_ZeroQuantity_ThrowsArgumentException()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 0
                }
            }
        };

        _repository
            .Setup(r => r.GetCustomerAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer
            {
                Id = 1,
                Name = "Test Customer"
            });

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateOrderAsync(
                request,
                CancellationToken.None));
    }
    // Test: validation rejects orders with invalid product ID
    [Fact]
    public async Task CreateOrder_InvalidProductId_ThrowsArgumentException()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest
                {
                    ProductId = 0,
                    Quantity = 1
                }
            }
        };

        _repository
            .Setup(r => r.GetCustomerAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer
            {
                Id = 1,
                Name = "Test Customer"
            });

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateOrderAsync(
                request,
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateOrder_ProductNotFound_ThrowsKeyNotFoundException()
    {
        _repository
            .Setup(x => x.GetCustomerAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer
            {
                Id = 1,
                Name = "Test Customer"
            });

        _repository
            .Setup(x => x.GetProductAsync(
                99,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = 99,
                    Quantity = 1
                }
            ]
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateOrderAsync(
                request,
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_ReturnsOrderResult()
    {
        _repository
            .Setup(x => x.GetCustomerAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer
            {
                Id = 1,
                Name = "Test Customer",
                IsVip = false
            });

        _repository
            .Setup(x => x.GetProductAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100,
                Stock = 10
            });

        _repository
            .Setup(x => x.AddOrderAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) =>
            {
                order.Id = 1;
            })
            .Returns(Task.CompletedTask);

        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 2
                }
            ]
        };

        var result = await _service.CreateOrderAsync(
            request,
            CancellationToken.None);

        Assert.Equal(1, result.OrderId);
        Assert.Equal("Test Customer", result.CustomerName);
        Assert.Equal(200, result.Total);
        Assert.Single(result.Items);
    }
    // Test: validation rejects orders with negative quantity
    [Fact]
    public async Task CreateOrder_NegativeQuantity_ThrowsArgumentException()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Items = new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = -1
                }
            }
        };

        _repository
            .Setup(r => r.GetCustomerAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer
            {
                Id = 1,
                Name = "Test Customer"
            });

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateOrderAsync(
                request,
                CancellationToken.None));
    }
}