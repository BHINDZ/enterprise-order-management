using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderPortal.Api.Data;
using OrderPortal.Api.Models;
using OrderPortal.Api.Services;

namespace OrderPortal.Api.Tests;

public class OrderServiceTests
{
    private static OrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }

    private static OrderService CreateService(
        OrderDbContext db,
        Mock<ITelecomApiClient> telecomApiClient)
    {
        var logger = Mock.Of<ILogger<OrderService>>();

        return new OrderService(
            db,
            telecomApiClient.Object,
            logger);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_SubmitsSuccessfully()
    {
        await using var db = CreateDbContext();

        var telecomApiClient =
            new Mock<ITelecomApiClient>();

        telecomApiClient
            .Setup(x => x.SubmitOrderAsync(
                It.IsAny<TelecomOrderRequest>()))
            .ReturnsAsync(
                new TelecomOrderResponse(
                    "TEL-TEST-001",
                    "SUBMITTED"));

        var service =
            CreateService(db, telecomApiClient);

        var request = new CreateOrderRequest
        {
            CustomerName = "Test Customer",
            ProductCode = "PLAN-100",
            Quantity = 1
        };

        var result =
            await service.CreateOrderAsync(request);

        Assert.Equal(
            OrderStatus.Submitted,
            result.Status);

        Assert.Equal(
            "TEL-TEST-001",
            result.ExternalOrderId);

        var savedOrder =
            await db.Orders.SingleAsync();

        Assert.Equal(
            OrderStatus.Submitted,
            savedOrder.Status);

        Assert.Equal(
            "TEL-TEST-001",
            savedOrder.ExternalOrderId);

        telecomApiClient.Verify(
            x => x.SubmitOrderAsync(
                It.IsAny<TelecomOrderRequest>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_ExternalApiReturnsCompleted_SetsCompletedStatus()
    {
        await using var db = CreateDbContext();

        var telecomApiClient =
            new Mock<ITelecomApiClient>();

        telecomApiClient
            .Setup(x => x.SubmitOrderAsync(
                It.IsAny<TelecomOrderRequest>()))
            .ReturnsAsync(
                new TelecomOrderResponse(
                    "TEL-TEST-002",
                    "COMPLETED"));

        var service =
            CreateService(db, telecomApiClient);

        var request = new CreateOrderRequest
        {
            CustomerName = "Completed Customer",
            ProductCode = "PLAN-200",
            Quantity = 2
        };

        var result =
            await service.CreateOrderAsync(request);

        Assert.Equal(
            OrderStatus.Completed,
            result.Status);

        Assert.Equal(
            "TEL-TEST-002",
            result.ExternalOrderId);
    }

    [Fact]
    public async Task CreateOrderAsync_ExternalApiFails_SetsFailedStatus()
    {
        await using var db = CreateDbContext();

        var telecomApiClient =
            new Mock<ITelecomApiClient>();

        telecomApiClient
            .Setup(x => x.SubmitOrderAsync(
                It.IsAny<TelecomOrderRequest>()))
            .ThrowsAsync(
                new HttpRequestException(
                    "Simulated telecom API failure"));

        var service =
            CreateService(db, telecomApiClient);

        var request = new CreateOrderRequest
        {
            CustomerName = "Failed Customer",
            ProductCode = "PLAN-300",
            Quantity = 1
        };

        var result =
            await service.CreateOrderAsync(request);

        Assert.Equal(
            OrderStatus.Failed,
            result.Status);

        var savedOrder =
            await db.Orders.SingleAsync();

        Assert.Equal(
            OrderStatus.Failed,
            savedOrder.Status);
    }

    [Fact]
    public async Task CreateOrderAsync_MissingCustomerName_ThrowsValidationException()
    {
        await using var db = CreateDbContext();

        var telecomApiClient =
            new Mock<ITelecomApiClient>();

        var service =
            CreateService(db, telecomApiClient);

        var request = new CreateOrderRequest
        {
            CustomerName = "",
            ProductCode = "PLAN-100",
            Quantity = 1
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateOrderAsync(request));

        Assert.Empty(db.Orders);
    }

    [Fact]
    public async Task CreateOrderAsync_InvalidQuantity_ThrowsValidationException()
    {
        await using var db = CreateDbContext();

        var telecomApiClient =
            new Mock<ITelecomApiClient>();

        var service =
            CreateService(db, telecomApiClient);

        var request = new CreateOrderRequest
        {
            CustomerName = "Test Customer",
            ProductCode = "PLAN-100",
            Quantity = 0
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateOrderAsync(request));

        Assert.Empty(db.Orders);
    }
}