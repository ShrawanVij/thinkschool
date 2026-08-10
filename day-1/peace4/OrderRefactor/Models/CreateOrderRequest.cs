namespace OrderRefactor.Models;

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