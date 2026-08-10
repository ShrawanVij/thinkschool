#if false
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrderRefactor.Legacy;

public class OrderController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly ILogger<OrderController> _logger;

    public OrderController(OrderDbContext db, ILogger<OrderController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost("/api/orders")]
    public async Task<object> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            if (request == null)
            {
                return new { success = false, message = "Request is required" };
            }

            if (request.CustomerId <= 0)
            {
                return new { success = false, message = "Invalid customer" };
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return new { success = false, message = "Order must contain items" };
            }

            var customer = _db.Customers
                .FirstOrDefault(c => c.Id == request.CustomerId);

            if (customer == null)
            {
                return new { success = false, message = "Customer not found" };
            }

            decimal total = 0;

            foreach (var item in request.Items)
            {
                if (item.ProductId <= 0)
                {
                    return new { success = false, message = "Invalid product" };
                }

                if (item.Quantity <= 0)
                {
                    return new { success = false, message = "Invalid quantity" };
                }

                var product = _db.Products
                    .FirstOrDefault(p => p.Id == item.ProductId);

                if (product == null)
                {
                    return new { success = false, message = "Product not found" };
                }

                if (product.Stock < item.Quantity)
                {
                    return new
                    {
                        success = false,
                        message = $"Not enough stock for {product.Name}"
                    };
                }

                var lineTotal = product.Price * item.Quantity;

                if (item.Quantity > 10)
                {
                    lineTotal = lineTotal * 0.90m;
                }

                total += lineTotal;
            }

            if (total > 1000)
            {
                total = total - 50;
            }

            if (customer.IsVip)
            {
                total = total * 0.95m;
            }

            var order = new Order
            {
                CustomerId = customer.Id,
                CreatedAt = DateTime.UtcNow,
                Total = total,
                Status = "Pending"
            };

            _db.Orders.Add(order);

            try
            {
                _db.SaveChanges();
            }
            catch
            {
            }

            foreach (var item in request.Items)
            {
                var product = _db.Products
                    .FirstOrDefault(p => p.Id == item.ProductId);

                try
                {
                    product.Stock -= item.Quantity;
                    _db.Products.Update(product);
                    _db.SaveChanges();
                }
                catch
                {
                }

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                _db.OrderItems.Add(orderItem);

                try
                {
                    _db.SaveChanges();
                }
                catch
                {
                }
            }

            try
            {
                var audit = new OrderAudit
                {
                    OrderId = order.Id,
                    Action = "OrderCreated",
                    CreatedAt = DateTime.UtcNow,
                    Details = "Order created successfully"
                };

                _db.OrderAudits.Add(audit);
                _db.SaveChanges();
            }
            catch
            {
            }

            if (request.ApplyCoupon)
            {
                var coupon = _db.Coupons
                    .FirstOrDefault(c => c.Code == request.CouponCode);

                if (coupon != null)
                {
                    if (coupon.ExpiryDate > DateTime.UtcNow)
                    {
                        order.Total = order.Total - coupon.DiscountAmount;
                        _db.SaveChanges();
                    }
                }
            }

            if (order.Total < 0)
            {
                order.Total = 0;
            }

            if (request.Items.Count > 1)
            {
                for (int i = 0; i <= request.Items.Count; i++)
                {
                    var currentItem = request.Items[i];

                    if (currentItem.Quantity > 5)
                    {
                        _logger.LogInformation(
                            "Large quantity item {ProductId}",
                            currentItem.ProductId);
                    }
                }
            }

            if (request.SendEmail)
            {
                try
                {
                    var email = customer.Email.ToLower();

                    _logger.LogInformation(
                        "Sending confirmation email to {Email}",
                        email);

                    // Email sending would happen here.
                }
                catch
                {
                }
            }

            var responseItems = new List<object>();

            foreach (var item in request.Items)
            {
                var product = _db.Products
                    .FirstOrDefault(p => p.Id == item.ProductId);

                responseItems.Add(new
                {
                    productId = product.Id,
                    productName = product.Name,
                    quantity = item.Quantity,
                    price = product.Price,
                    total = product.Price * item.Quantity
                });
            }

            return new
            {
                success = true,
                orderId = order.Id,
                customer = customer.Name,
                total = order.Total,
                status = order.Status,
                items = responseItems
            };
        }
        catch
        {
        }

        return new
        {
            success = false,
            message = "Unable to create order"
        };
    }
}

public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
    public bool ApplyCoupon { get; set; }
    public string? CouponCode { get; set; }
    public bool SendEmail { get; set; }
}

public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "";
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderAudit
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Action { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string Details { get; set; } = "";
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public bool IsVip { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public decimal DiscountAmount { get; set; }
    public DateTime ExpiryDate { get; set; }
}

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderAudit> OrderAudits => Set<OrderAudit>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
}
#endif