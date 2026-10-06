namespace OrderPortal.Api.Models;

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
