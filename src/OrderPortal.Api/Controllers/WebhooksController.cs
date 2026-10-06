using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderPortal.Api.Data;
using OrderPortal.Api.Models;

namespace OrderPortal.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController(
    OrderDbContext db,
    IConfiguration configuration,
    ILogger<WebhooksController> logger) : ControllerBase
{
    [HttpPost("telecom-order")]
    public async Task<IActionResult> TelecomOrderWebhook(
        TelecomOrderWebhookRequest request)
    {
        var signature =
            Request.Headers["X-Webhook-Signature"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(signature))
        {
            logger.LogWarning(
                "Webhook rejected because signature is missing.");

            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Webhook signature required",
                Detail = "The webhook signature is missing."
            });
        }

        var webhookSecret =
            configuration["TelecomApi:WebhookSecret"];

        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            logger.LogError(
                "Webhook secret is not configured.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Webhook configuration error"
                });
        }

        var payload =
            $"{request.ExternalOrderId}|{request.Status}";

        var expectedSignature =
            CreateSignature(
                payload,
                webhookSecret);

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature),
                Encoding.UTF8.GetBytes(expectedSignature)))
        {
            logger.LogWarning(
                "Webhook rejected because signature is invalid for {ExternalOrderId}.",
                request.ExternalOrderId);

            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid webhook signature",
                Detail = "The webhook signature could not be verified."
            });
        }

        if (string.IsNullOrWhiteSpace(request.ExternalOrderId))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid webhook",
                Detail = "ExternalOrderId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid webhook",
                Detail = "Status is required."
            });
        }

        logger.LogInformation(
            "Received verified telecom webhook for {ExternalOrderId}. Status: {Status}",
            request.ExternalOrderId,
            request.Status);

        var order = await db.Orders
            .FirstOrDefaultAsync(o =>
                o.ExternalOrderId == request.ExternalOrderId);

        if (order is null)
        {
            logger.LogWarning(
                "Order not found for external order ID {ExternalOrderId}.",
                request.ExternalOrderId);

            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Order not found",
                Detail =
                    $"No order was found for external order ID '{request.ExternalOrderId}'."
            });
        }

        order.Status = request.Status.ToUpperInvariant() switch
        {
            "SUBMITTED" => OrderStatus.Submitted,
            "PROCESSING" => OrderStatus.Processing,
            "COMPLETED" => OrderStatus.Completed,
            "FAILED" => OrderStatus.Failed,
            _ => order.Status
        };

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Order {OrderId} updated from verified telecom webhook. New status: {Status}",
            order.Id,
            order.Status);

        return Ok(new
        {
            message = "Webhook processed successfully.",
            orderId = order.Id,
            externalOrderId = order.ExternalOrderId,
            status = order.Status.ToString()
        });
    }

    private static string CreateSignature(
        string payload,
        string secret)
    {
        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(secret));

        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(hash);
    }
}