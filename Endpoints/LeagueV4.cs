using System.Text.Json.Serialization;
using Statikk_Data.DTOs.RiotApi;
using Statikk_Data.DTOs.RiotApi.LeagueV4;
using Statikk_Data.ENUMs;
using Statikk_Data.Features.RiotApiClient;

namespace Statikk_Data.Endpoints;

public sealed class LeagueV4(
    RiotApiClient riotApiClient
)
{
    public async Task<RiotApiLeagueEntryList?> GetChallengerLeagueEntriesAsync(
        PlatformRoute platformRoute,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "lol/league/v4/challengerleagues/by-queue/RANKED_SOLO_5x5";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(platformRoute));
        
        url.AppendPath(path);
        
        return await riotApiClient.SendAsync(
            platformRoute,
            Methods.GetChallengerLeagueEntriesAsync,
            url.ToString(),
            LeagueV4JsonContext.Default.RiotApiLeagueEntryList,
            cancellationToken
        ).ConfigureAwait(false);
    }
    
    public async Task<RiotApiLeagueEntry[]> GetLeagueEntriesByTierAndDivisionAsync(
        PlatformRoute platformRoute,
        Tier tier,
        Division division,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "lol/league/v4/entries/RANKED_SOLO_5x5";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(platformRoute));
        
        url.AppendPath(path);
        url.AppendPath(tier.GetStringUpperCase());
        url.AppendPath(division.GetEnumMemberValue());
        url.AppendQuery(nameof(page), page);
        
        return await riotApiClient.SendAsync(
            platformRoute,
            Methods.GetLeagueEntriesByTierAndDivisionForV4Async,
            url.ToString(),
            LeagueV4JsonContext.Default.RiotApiLeagueEntryArray,
            cancellationToken
        ).ConfigureAwait(false) ?? [];
    }
    
    public async Task<RiotApiLeagueEntryList?> GetGrandMasterLeagueEntriesAsync(
        PlatformRoute platformRoute,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "lol/league/v4/grandmasterleagues/by-queue/RANKED_SOLO_5x5";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(platformRoute));
        
        url.AppendPath(path);
        
        return await riotApiClient.SendAsync(
            platformRoute,
            Methods.GetGrandMasterLeagueEntriesAsync,
            url.ToString(),
            LeagueV4JsonContext.Default.RiotApiLeagueEntryList,
            cancellationToken
        ).ConfigureAwait(false);
    }
    
    public async Task<RiotApiLeagueEntryList?> GetMasterLeagueEntriesAsync(
        PlatformRoute platformRoute,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "lol/league/v4/masterleagues/by-queue/RANKED_SOLO_5x5";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(platformRoute));
        
        url.AppendPath(path);
        
        return await riotApiClient.SendAsync(
            platformRoute,
            Methods.GetMasterLeagueEntriesAsync,
            url.ToString(),
            LeagueV4JsonContext.Default.RiotApiLeagueEntryList,
            cancellationToken
        ).ConfigureAwait(false);
    }
}

[JsonSerializable(typeof(RiotApiLeagueEntryList))]
[JsonSerializable(typeof(RiotApiLeagueEntry[]))]
internal partial class LeagueV4JsonContext : JsonSerializerContext;