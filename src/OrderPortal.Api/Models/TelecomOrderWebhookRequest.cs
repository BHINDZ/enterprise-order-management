namespace OrderPortal.Api.Models;

public class TelecomOrderWebhookRequest
{
    public string ExternalOrderId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}