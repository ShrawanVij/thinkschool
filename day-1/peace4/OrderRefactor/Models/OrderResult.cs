namespace OrderRefactor.Models;

public class OrderResult
{
    public int OrderId { get; set; }

    public string CustomerName { get; set; } = "";

    public decimal Total { get; set; }

    public string Status { get; set; } = "";

    public List<OrderItemResult> Items { get; set; } = new();
}

public class OrderItemResult
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = "";

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Total { get; set; }
}