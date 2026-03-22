using Microsoft.Extensions.DependencyInjection;
using Statikk_Data.Workers;

namespace Statikk_Data.Helpers;

public static class StatikkDataServiceCollectionExtensions
{
    public static IServiceCollection AddStatikkDataService(
        this IServiceCollection services
    )
    {
        services.AddHttpClient("StatikkDataClient", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mosgi/1.0");
        });
        
        services.AddSingleton<StatikkData>();
        services.AddHostedService(sp => sp.GetRequiredService<StatikkData>());
        
        return services;
    }
}