using Statikk_Data.DTOs;
using Statikk_Data.ENUMs;

namespace Statikk_Data.Endpoints;

public sealed class LeagueExpV4(
    RiotApiClient riotApiClient
)
{
    public async Task<LeagueEntry[]> GetLeagueEntriesByTierAndDivisionAsync(
        PlatformRoute platformRoute,
        Tier tier,
        Division division,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        return await riotApiClient.SendAsync<LeagueEntry[]>(
            platformRoute,
            nameof(GetLeagueEntriesByTierAndDivisionAsync),
            $"lol/league-exp/v4/entries/RANKED_SOLO_5x5/{tier.GetStringUpperCase()}/{division.GetEnumMemberValue()}?page={page}",
            cancellationToken
        ) ?? [];
    }
}