using System.Text.Json.Serialization;
using Statikk_Data.DTOs.RiotApi.AccountV1;
using Statikk_Data.ENUMs;
using Statikk_Data.Features.RiotApiClient;

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
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(RegionalRoute.Europe));
        
        url.AppendPath(path);
        url.AppendPath(puuid);
        
        return await riotApiClient.SendAsync(
            RegionalRoute.Europe,
            Methods.GetAccountByPuuidAsync,
            url.ToString(),
            AccountV1JsonContext.Default.RiotApiAccountDto,
            cancellationToken
        ).ConfigureAwait(false);
    }
    
    public async Task<RiotApiAccountDto?> GetAccountByRiotIdAsync(
        string gameName,
        string tagLine,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "riot/account/v1/accounts/by-riot-id";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(RegionalRoute.Europe));
        
        url.AppendPath(path);
        url.AppendPath(gameName);
        url.AppendPath(tagLine);

        return await riotApiClient.SendAsync(
            RegionalRoute.Europe,
            Methods.GetAccountByRiotIdAsync,
            url.ToString(),
            AccountV1JsonContext.Default.RiotApiAccountDto,
            cancellationToken
        ).ConfigureAwait(false);
    }
    
    public async Task<RiotApiAccountRegionDto?> GetAccountRegionByPuuidAsync(
        string puuid,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "riot/account/v1/region/by-game/lol/by-puuid";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(RegionalRoute.Europe));
        
        url.AppendPath(path);
        url.AppendPath(puuid);

        return await riotApiClient.SendAsync(
            RegionalRoute.Europe,
            Methods.GetAccountRegionByPuuidAsync,
            url.ToString(),
            AccountV1JsonContext.Default.RiotApiAccountRegionDto,
            cancellationToken
        ).ConfigureAwait(false);
    }
}

[JsonSerializable(typeof(RiotApiAccountDto))]
[JsonSerializable(typeof(RiotApiAccountRegionDto))]
internal partial class AccountV1JsonContext : JsonSerializerContext;