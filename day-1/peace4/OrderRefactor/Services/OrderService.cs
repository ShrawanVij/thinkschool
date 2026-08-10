using OrderRefactor.Models;
using OrderRefactor.Repositories;

namespace OrderRefactor.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OrderResult> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CustomerId <= 0)
            throw new ArgumentException("Invalid customer.");

        if (request.Items.Count == 0)
            throw new ArgumentException("Order must contain items.");

        var customer = await _repository.GetCustomerAsync(
            request.CustomerId,
            cancellationToken);

        if (customer is null)
            throw new KeyNotFoundException("Customer not found.");

        decimal total = 0;

        var products = new List<(CreateOrderItemRequest Item, Product Product)>();

        foreach (var item in request.Items)
        {
            if (item.ProductId <= 0)
                throw new ArgumentException("Invalid product.");

            if (item.Quantity <= 0)
                throw new ArgumentException("Invalid quantity.");

            var product = await _repository.GetProductAsync(
                item.ProductId,
                cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("Product not found.");

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException(
                    $"Not enough stock for {product.Name}.");

            var lineTotal = product.Price * item.Quantity;

            if (item.Quantity > 10)
                lineTotal *= 0.90m;

            total += lineTotal;

            products.Add((item, product));
        }

        if (total > 1000)
            total -= 50;

        if (customer.IsVip)
            total *= 0.95m;

        var order = new Order
        {
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow,
            Total = total,
            Status = "Pending"
        };

        await _repository.AddOrderAsync(
            order,
            cancellationToken);

        foreach (var entry in products)
        {
            entry.Product.Stock -= entry.Item.Quantity;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = entry.Item.ProductId,
                Quantity = entry.Item.Quantity,
                UnitPrice = entry.Product.Price
            };

            await _repository.AddOrderItemAsync(
                orderItem,
                cancellationToken);
        }

        var audit = new OrderAudit
        {
            OrderId = order.Id,
            Action = "OrderCreated",
            CreatedAt = DateTime.UtcNow,
            Details = "Order created successfully"
        };

        await _repository.AddAuditAsync(
            audit,
            cancellationToken);

        if (request.ApplyCoupon &&
            !string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _repository.GetCouponAsync(
                request.CouponCode,
                cancellationToken);

            if (coupon is not null &&
                coupon.ExpiryDate > DateTime.UtcNow)
            {
                order.Total = Math.Max(
                    0,
                    order.Total - coupon.DiscountAmount);
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created order {OrderId} for customer {CustomerId}",
            order.Id,
            customer.Id);

        var responseItems = products.Select(x => new OrderItemResult
        {
            ProductId = x.Product.Id,
            ProductName = x.Product.Name,
            Quantity = x.Item.Quantity,
            Price = x.Product.Price,
            Total = x.Product.Price * x.Item.Quantity
        }).ToList();

        return new OrderResult
        {
            OrderId = order.Id,
            CustomerName = customer.Name,
            Total = order.Total,
            Status = order.Status,
            Items = responseItems
        };
    }
}