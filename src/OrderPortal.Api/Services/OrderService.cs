using Microsoft.EntityFrameworkCore;
using OrderPortal.Api.Data;
using OrderPortal.Api.Models;

namespace OrderPortal.Api.Services;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _db;
    private readonly ITelecomApiClient _telecomApiClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        OrderDbContext db,
        ITelecomApiClient telecomApiClient,
        ILogger<OrderService> logger)
    {
        _db = db;
        _telecomApiClient = telecomApiClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersAsync()
    {
        return await _db.Orders
            .OrderByDescending(o => o.CreatedUtc)
            .ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ArgumentException("Customer name is required.");

        if (string.IsNullOrWhiteSpace(request.ProductCode))
            throw new ArgumentException("Product code is required.");

        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var order = new Order
        {
            CustomerName = request.CustomerName,
            ProductCode = request.ProductCode,
            Quantity = request.Quantity,
            Status = OrderStatus.Pending,
            CreatedUtc = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        try
        {
            _logger.LogInformation(
                "Submitting order {OrderId} to telecom API.",
                order.Id);

            var telecomRequest = new TelecomOrderRequest(
                order.CustomerName,
                order.ProductCode,
                order.Quantity);

            var response =
                await _telecomApiClient.SubmitOrderAsync(telecomRequest);

            order.ExternalOrderId = response.ExternalOrderId;

            order.Status = response.Status?.ToUpperInvariant() switch
            {
                "SUBMITTED" => OrderStatus.Submitted,
                "PROCESSING" => OrderStatus.Processing,
                "COMPLETED" => OrderStatus.Completed,
                "FAILED" => OrderStatus.Failed,
                _ => OrderStatus.Pending
            };

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} successfully submitted. ExternalOrderId: {ExternalOrderId}, Status: {Status}",
                order.Id,
                order.ExternalOrderId,
                order.Status);

            return order;
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;

            await _db.SaveChangesAsync();

            _logger.LogError(
                ex,
                "Failed to submit order {OrderId} to the telecom API.",
                order.Id);

            return order;
        }
    }
}