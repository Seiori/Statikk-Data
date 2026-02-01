using Statikk_Data.DTOs;
using Statikk_Data.ENUMs;

namespace Statikk_Data.Endpoints;

public sealed class MatchV5(
    RiotApiClient riotApiClient
)
{
    public async Task<string[]> GetMatchIdsByPuuid(
        RegionalRoute regionalRoute,
        string puuid,
        int start = 0,
        int count = 20,
        CancellationToken cancellationToken = default
    )
    {
        return await riotApiClient.SendAsync<string[]>(
            regionalRoute,
            nameof(GetMatchIdsByPuuid),
            $"lol/match/v5/matches/by-puuid/{puuid}/ids?queue=420&start={start}&count={count}",
            cancellationToken
        ) ?? [];
    }

    public async Task<Match?> GetMatchByMatchId(
        RegionalRoute regionalRoute,
        string matchId,
        CancellationToken cancellationToken = default
    )
    {
        return await riotApiClient.SendAsync<Match>(
            regionalRoute,
            nameof(GetMatchByMatchId),
            $"lol/match/v5/matches/{matchId}",
            cancellationToken
        );
    }
}