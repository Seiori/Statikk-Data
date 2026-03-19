using Statikk_Data.DTOs.RiotApi;
using Statikk_Data.ENUMs;

namespace Statikk_Data.Endpoints;

public sealed class SummonerV4(
    RiotApiClient riotApiClient
)
{
    public async Task<RiotApiSummonerDto?> GetSummonerByPuuidAsync(
        PlatformRoute platformRoute,
        string puuid,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "lol/summoner/v4/summoners/by-puuid";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(platformRoute));
        
        url.AppendPath(path);
        url.AppendPath(puuid);
        
        return await riotApiClient.SendAsync(
            platformRoute,
            Methods.GetSummonerByPuuidAsync,
            url.ToString(),
            RiotApiJsonContext.Default.RiotApiSummonerDto,
            cancellationToken
        );
    }
}