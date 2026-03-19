using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Statikk_Data;

public static class RiotApiServiceCollectionExtensions
{
    private const string RiotApiKeyHeader = "X-Riot-Token";
    
    public static IServiceCollection AddRiotApiClient(
        this IServiceCollection services,
        string riotApiKey
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(riotApiKey);

        services.AddHttpClient<RiotApiClient>(httpClientBuilder =>
        {
            httpClientBuilder.DefaultRequestHeaders.Add(RiotApiKeyHeader, riotApiKey);
        }
        ).ConfigurePrimaryHttpMessageHandler(
            () => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
                AutomaticDecompression = DecompressionMethods.All,
                UseProxy = false
            }
        );
        
        return services;
    }
}