using Microsoft.AspNetCore.Mvc;
using OrderPortal.Api.Models;

namespace OrderPortal.Api.Controllers;

[ApiController]
[Route("api/telecom")]
public class TelecomSimulatorController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<TelecomSimulatorController> logger) : ControllerBase
{
    [HttpPost("orders")]
    public async Task<ActionResult<TelecomOrderResponse>> CreateOrder(
        TelecomOrderRequest request)
    {
        // Simulate external API processing time.
        await Task.Delay(500);

        var externalOrderId =
            $"TEL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

        var response = new TelecomOrderResponse(
            externalOrderId,
            "SUBMITTED");

        logger.LogInformation(
            "Telecom simulator created external order {ExternalOrderId}.",
            externalOrderId);

        // Simulate asynchronous processing.
        _ = ProcessOrderAsync(externalOrderId);

        return Ok(response);
    }

    private async Task ProcessOrderAsync(string externalOrderId)
    {
        try
        {
            // Simulate the external system processing the order.
            await Task.Delay(5000);

            var webhookUrl =
                configuration["TelecomApi:WebhookUrl"];

            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                logger.LogWarning(
                    "Telecom webhook URL is not configured.");

                return;
            }

            var client =
                httpClientFactory.CreateClient("WebhookClient");

            var webhookRequest =
     new TelecomOrderWebhookRequest
     {
         ExternalOrderId = externalOrderId,
         Status = "COMPLETED"
     };

            var webhookSecret =
                configuration["TelecomApi:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                logger.LogWarning(
                    "Webhook secret is not configured.");

                return;
            }

            var payload =
                $"{webhookRequest.ExternalOrderId}|{webhookRequest.Status}";

            var signature =
                CreateSignature(
                    payload,
                    webhookSecret);

            logger.LogInformation(
                "Sending signed webhook for {ExternalOrderId}. Status: COMPLETED",
                externalOrderId);

            var webhookMessage =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    webhookUrl)
                {
                    Content = JsonContent.Create(
                        webhookRequest)
                };

            webhookMessage.Headers.Add(
                "X-Webhook-Signature",
                signature);

            var webhookResponse =
                await client.SendAsync(
                    webhookMessage);

            if (webhookResponse.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "Webhook successfully delivered for {ExternalOrderId}.",
                    externalOrderId);
            }
            else
            {
                logger.LogWarning(
                    "Webhook delivery failed for {ExternalOrderId}. HTTP {StatusCode}",
                    externalOrderId,
                    webhookResponse.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while processing webhook for {ExternalOrderId}.",
                externalOrderId);
        }
    }
    private static string CreateSignature(
    string payload,
    string secret)
    {
        using var hmac =
            new System.Security.Cryptography.HMACSHA256(
                System.Text.Encoding.UTF8.GetBytes(secret));

        var hash =
            hmac.ComputeHash(
                System.Text.Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(hash);
    }
}