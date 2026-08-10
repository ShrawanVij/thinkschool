using Microsoft.EntityFrameworkCore;
using OrderRefactor.Data;
using OrderRefactor.Models;

namespace OrderRefactor.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _db;

    public OrderRepository(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<Customer?> GetCustomerAsync(
        int customerId,
        CancellationToken cancellationToken)
    {
        return await _db.Customers
            .FirstOrDefaultAsync(
                x => x.Id == customerId,
                cancellationToken);
    }

    public async Task<Product?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        return await _db.Products
            .FirstOrDefaultAsync(
                x => x.Id == productId,
                cancellationToken);
    }

    public async Task<Coupon?> GetCouponAsync(
        string code,
        CancellationToken cancellationToken)
    {
        return await _db.Coupons
            .FirstOrDefaultAsync(
                x => x.Code == code,
                cancellationToken);
    }

    public Task AddOrderAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        return _db.Orders.AddAsync(order, cancellationToken).AsTask();
    }

    public Task AddOrderItemAsync(
        OrderItem orderItem,
        CancellationToken cancellationToken)
    {
        return _db.OrderItems.AddAsync(orderItem, cancellationToken).AsTask();
    }

    public Task AddAuditAsync(
        OrderAudit audit,
        CancellationToken cancellationToken)
    {
        return _db.OrderAudits.AddAsync(audit, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}