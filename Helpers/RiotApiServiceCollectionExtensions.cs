using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Statikk_Data.DTOs.RiotApi.RiotApiClient;
using Statikk_Data.Features.RiotApiClient;

namespace Statikk_Data.Helpers;

public static class RiotApiServiceCollectionExtensions
{
    public static IServiceCollection AddRiotApiClient(
        this IServiceCollection services,
        string riotApiKey,
        int requests,
        int seconds
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(riotApiKey);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requests);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seconds);
        
        services.AddSingleton(new RiotApiClientSettings(requests, seconds));

        services.AddHttpClient<RiotApiClient>(httpClientBuilder =>
        {
            httpClientBuilder.DefaultRequestHeaders.Add("X-Riot-Token", riotApiKey);
        }
        ).ConfigurePrimaryHttpMessageHandler(
            () => new SocketsHttpHandler
            {
                EnableMultipleHttp3Connections = true,
                AutomaticDecompression = DecompressionMethods.All,
                UseProxy = false
            }
        );
        
        return services;
    }
}