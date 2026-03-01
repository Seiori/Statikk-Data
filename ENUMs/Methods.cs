namespace Statikk_Data.ENUMs;

public enum Methods : byte
{
    // Account V1
    GetAccountByPuuidAsync,
    GetAccountByRiotIdAsync,
    GetAccountRegionByPuuidAsync,
    
    // LeagueExp V4
    GetLeagueEntriesByTierAndDivisionForExpV4Async,
    
    // League V4
    GetChallengerLeagueEntriesAsync,
    GetLeagueEntryByPuuidAsync,
    GetLeagueEntriesByTierAndDivisionForV4Async,
    GetGrandMasterLeagueEntriesAsync,
    GetMasterLeagueEntriesAsync,
    
    // Match V5
    GetMatchIdsByPuuidAsync,
    GetMatchByMatchIdAsync,
    
    // Summoner V4
    GetSummonerByPuuidAsync
}