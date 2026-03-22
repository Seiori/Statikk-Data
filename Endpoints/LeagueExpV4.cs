using System.Text.Json.Serialization;
using Statikk_Data.DTOs.RiotApi.LeagueV4;
using Statikk_Data.ENUMs;
using Statikk_Data.Features.RiotApiClient;

namespace Statikk_Data.Endpoints;

public sealed class LeagueExpV4(
    RiotApiClient riotApiClient
)
{
    public async Task<RiotApiLeagueEntry[]> GetLeagueEntriesByTierAndDivisionAsync(
        PlatformRoute platformRoute,
        Tier tier,
        Division division,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        const string path = "lol/league-exp/v4/entries/RANKED_SOLO_5x5";
        Span<char> buffer = stackalloc char[256];
        var url = new RiotUrlBuilder(buffer, RiotApi.GetBaseUrl(platformRoute));
        
        url.AppendPath(path);
        url.AppendPath(tier.GetStringUpperCase());
        url.AppendPath(division.GetEnumMemberValue());
        url.AppendQuery(nameof(page), page);
        
        return await riotApiClient.SendAsync(
            platformRoute,
            Methods.GetLeagueEntriesByTierAndDivisionForExpV4Async,
            url.ToString(),
            LeagueExpV4JsonContext.Default.RiotApiLeagueEntryArray,
            cancellationToken
        ).ConfigureAwait(false) ?? [];
    }
}

[JsonSerializable(typeof(RiotApiLeagueEntry[]))]
internal partial class LeagueExpV4JsonContext : JsonSerializerContext;