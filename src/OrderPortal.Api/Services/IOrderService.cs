using OrderPortal.Api.Models;

namespace OrderPortal.Api.Services;

public interface IOrderService
{
    Task<IReadOnlyList<Order>> GetOrdersAsync();
    Task<Order> CreateOrderAsync(CreateOrderRequest request);
}
