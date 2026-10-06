namespace OrderPortal.Api.Models;

public enum OrderStatus
{
    Pending,
    Submitted,
    Processing,
    Completed,
    Failed
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? ExternalOrderId { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
