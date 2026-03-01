using Statikk_Data.DTOs.RiotApi;
using Statikk_Data.ENUMs;

namespace Statikk_Data.Endpoints;

public sealed class AccountV1(
    RiotApiClient riotApiClient
)
{
    public async Task<RiotApiAccountDto?> GetAccountByPuuidAsync(
        string puuid,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "riot/account/v1/accounts/by-puuid";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetUrl(RegionalRoute.Europe));
        
        url.AppendPath(path);
        url.AppendPath(puuid);
        
        return await riotApiClient.SendAsync(
            RegionalRoute.Europe,
            Methods.GetAccountByPuuidAsync,
            url.ToString(),
            RiotApiJsonContext.Default.RiotApiAccountDto,
            cancellationToken
        );
    }
    
    public async Task<RiotApiAccountDto?> GetAccountByRiotIdAsync(
        string gameName,
        string tagLine,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "riot/account/v1/accounts/by-riot-id";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetUrl(RegionalRoute.Europe));
        
        url.AppendPath(path);
        url.AppendPath(gameName);
        url.AppendPath(tagLine);

        return await riotApiClient.SendAsync(
            RegionalRoute.Europe,
            Methods.GetAccountByRiotIdAsync,
            url.ToString(),
            RiotApiJsonContext.Default.RiotApiAccountDto,
            cancellationToken
        );
    }
    
    public async Task<RiotApiAccountRegionDto?> GetAccountRegionByPuuidAsync(
        string puuid,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "riot/account/v1/region/by-game/lol/by-puuid";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetUrl(RegionalRoute.Europe));
        
        url.AppendPath(path);
        url.AppendPath(puuid);

        return await riotApiClient.SendAsync(
            RegionalRoute.Europe,
            Methods.GetAccountRegionByPuuidAsync,
            url.ToString(),
            RiotApiJsonContext.Default.RiotApiAccountRegionDto,
            cancellationToken
        );
    }
}