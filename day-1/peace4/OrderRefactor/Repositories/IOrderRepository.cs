using OrderRefactor.Models;

namespace OrderRefactor.Repositories;

public interface IOrderRepository
{
    Task<Customer?> GetCustomerAsync(
        int customerId,
        CancellationToken cancellationToken);

    Task<Product?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken);

    Task<Coupon?> GetCouponAsync(
        string code,
        CancellationToken cancellationToken);

    Task AddOrderAsync(
        Order order,
        CancellationToken cancellationToken);

    Task AddOrderItemAsync(
        OrderItem orderItem,
        CancellationToken cancellationToken);

    Task AddAuditAsync(
        OrderAudit audit,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}