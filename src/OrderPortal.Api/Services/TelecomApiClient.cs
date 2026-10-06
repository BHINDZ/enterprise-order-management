using System.Net.Http.Json;
using OrderPortal.Api.Models;

namespace OrderPortal.Api.Services;

public class TelecomApiClient(HttpClient httpClient, ILogger<TelecomApiClient> logger)
    : ITelecomApiClient
{
    public async Task<TelecomOrderResponse> SubmitOrderAsync(
        TelecomOrderRequest request)
    {
        logger.LogInformation(
            "Submitting order for {Customer} to external telecom API",
            request.CustomerName);

        using var response = await httpClient.PostAsJsonAsync(
            "api/telecom/orders", request);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<TelecomOrderResponse>();

        return result ?? throw new InvalidOperationException(
            "The telecom API returned an empty response.");
    }
}
