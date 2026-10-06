namespace OrderPortal.Api.Models;

public record TelecomOrderRequest(
    string CustomerName,
    string ProductCode,
    int Quantity);
