using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OrderPortal.Api.Controllers;
using OrderPortal.Api.Data;
using OrderPortal.Api.Models;

namespace OrderPortal.Api.Tests;

public class WebhooksControllerTests
{
    private const string WebhookSecret = "test-webhook-secret";

    private static OrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["TelecomApi:WebhookSecret"] = WebhookSecret
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private static WebhooksController CreateController(
        OrderDbContext db)
    {
        var configuration = CreateConfiguration();
        var logger = Mock.Of<ILogger<WebhooksController>>();

        var controller = new WebhooksController(
            db,
            configuration,
            logger);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private static string CreateSignature(
        string externalOrderId,
        string status)
    {
        var payload =
            $"{externalOrderId}|{status}";

        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(WebhookSecret));

        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(hash);
    }

    [Fact]
    public async Task TelecomOrderWebhook_MissingSignature_ReturnsUnauthorized()
    {
        await using var db = CreateDbContext();

        var controller = CreateController(db);

        var request = new TelecomOrderWebhookRequest
        {
            ExternalOrderId = "TEL-TEST-001",
            Status = "COMPLETED"
        };

        var result =
            await controller.TelecomOrderWebhook(request);

        var unauthorized =
            Assert.IsType<UnauthorizedObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            unauthorized.StatusCode);
    }

    [Fact]
    public async Task TelecomOrderWebhook_InvalidSignature_ReturnsUnauthorized()
    {
        await using var db = CreateDbContext();

        var controller = CreateController(db);

        var request = new TelecomOrderWebhookRequest
        {
            ExternalOrderId = "TEL-TEST-002",
            Status = "COMPLETED"
        };

        controller.HttpContext.Request.Headers[
            "X-Webhook-Signature"] =
            "INVALID-SIGNATURE";

        var result =
            await controller.TelecomOrderWebhook(request);

        var unauthorized =
            Assert.IsType<UnauthorizedObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            unauthorized.StatusCode);
    }

    [Fact]
    public async Task TelecomOrderWebhook_ValidSignature_UpdatesOrderStatus()
    {
        await using var db = CreateDbContext();

        var order = new Order
        {
            CustomerName = "Webhook Test Customer",
            ProductCode = "PLAN-100",
            Quantity = 1,
            Status = OrderStatus.Submitted,
            ExternalOrderId = "TEL-TEST-003",
            CreatedUtc = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var request = new TelecomOrderWebhookRequest
        {
            ExternalOrderId = "TEL-TEST-003",
            Status = "COMPLETED"
        };

        var signature =
            CreateSignature(
                request.ExternalOrderId,
                request.Status);

        controller.HttpContext.Request.Headers[
            "X-Webhook-Signature"] =
            signature;

        var result =
            await controller.TelecomOrderWebhook(request);

        var ok =
            Assert.IsType<OkObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status200OK,
            ok.StatusCode);

        var updatedOrder =
            await db.Orders.SingleAsync();

        Assert.Equal(
            OrderStatus.Completed,
            updatedOrder.Status);
    }

    [Fact]
    public async Task TelecomOrderWebhook_UnknownOrder_ReturnsNotFound()
    {
        await using var db = CreateDbContext();

        var controller = CreateController(db);

        var request = new TelecomOrderWebhookRequest
        {
            ExternalOrderId = "TEL-DOES-NOT-EXIST",
            Status = "COMPLETED"
        };

        var signature =
            CreateSignature(
                request.ExternalOrderId,
                request.Status);

        controller.HttpContext.Request.Headers[
            "X-Webhook-Signature"] =
            signature;

        var result =
            await controller.TelecomOrderWebhook(request);

        var notFound =
            Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            notFound.StatusCode);
    }

    [Fact]
    public async Task TelecomOrderWebhook_ValidSignature_UpdatesFailedStatus()
    {
        await using var db = CreateDbContext();

        var order = new Order
        {
            CustomerName = "Failed Webhook Customer",
            ProductCode = "PLAN-500",
            Quantity = 1,
            Status = OrderStatus.Processing,
            ExternalOrderId = "TEL-TEST-005",
            CreatedUtc = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var request = new TelecomOrderWebhookRequest
        {
            ExternalOrderId = "TEL-TEST-005",
            Status = "FAILED"
        };

        var signature =
            CreateSignature(
                request.ExternalOrderId,
                request.Status);

        controller.HttpContext.Request.Headers[
            "X-Webhook-Signature"] =
            signature;

        var result =
            await controller.TelecomOrderWebhook(request);

        Assert.IsType<OkObjectResult>(result);

        var updatedOrder =
            await db.Orders.SingleAsync();

        Assert.Equal(
            OrderStatus.Failed,
            updatedOrder.Status);
    }
}