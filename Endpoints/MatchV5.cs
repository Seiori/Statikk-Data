using System.Text.Json.Serialization;
using Statikk_Data.DTOs.RiotApi;
using Statikk_Data.DTOs.RiotApi.MatchV5;
using Statikk_Data.ENUMs;
using Statikk_Data.Features.RiotApiClient;

namespace Statikk_Data.Endpoints;

public sealed class MatchV5(
    RiotApiClient riotApiClient
)
{
    public async Task<string[]> GetMatchIdsByPuuidAsync(
        RegionalRoute regionalRoute,
        string puuid,
        int start = 0,
        int count = 20,
        long startTime = 0,
        long endTime = 0,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "lol/match/v5/matches/by-puuid";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(regionalRoute));

        url.AppendPath(path);
        url.AppendPath(puuid);
        url.AppendPath("ids");
        url.AppendQuery("queue", 420); 
        url.AppendQuery("start", start); 
        url.AppendQuery("count", count);
        url.AppendQuery("startTime", startTime);
        url.AppendQuery("endTime", endTime);
        
        return await riotApiClient.SendAsync(
            regionalRoute,
            Methods.GetMatchIdsByPuuidAsync,
            url.ToString(),
            MatchV5JsonContext.Default.StringArray,
            cancellationToken
        ).ConfigureAwait(false) ?? [];
    }

    public async Task<RiotApiMatch?> GetMatchByMatchIdAsync(
        RegionalRoute regionalRoute,
        string matchId,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "lol/match/v5/matches";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(regionalRoute));
        
        url.AppendPath(path);
        url.AppendPath(matchId);
        
        return await riotApiClient.SendAsync(
            regionalRoute,
            Methods.GetMatchByMatchIdAsync,
            url.ToString(),
            MatchV5JsonContext.Default.RiotApiMatch,
            cancellationToken
        ).ConfigureAwait(false);
    }
}

[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(RiotApiMatch))]
internal partial class MatchV5JsonContext : JsonSerializerContext;