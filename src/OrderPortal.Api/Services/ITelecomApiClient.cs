using OrderPortal.Api.Models;

namespace OrderPortal.Api.Services;

public interface ITelecomApiClient
{
    Task<TelecomOrderResponse> SubmitOrderAsync(TelecomOrderRequest request);
}
