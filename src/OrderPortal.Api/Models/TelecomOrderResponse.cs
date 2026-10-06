namespace OrderPortal.Api.Models;

public record TelecomOrderResponse(
    string ExternalOrderId,
    string Status);
