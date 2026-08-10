namespace OrderRefactor.Models;

public class OrderAudit
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Action { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string Details { get; set; } = "";
}