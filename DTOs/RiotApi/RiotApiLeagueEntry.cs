using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi;

public readonly record struct RiotApiLeagueEntry(
    [property: JsonPropertyName("puuid")] string Puuid,
    [property: JsonPropertyName("leaguePoints")] short LeaguePoints,
    [property: JsonPropertyName("wins")] short Wins,
    [property: JsonPropertyName("losses")] short Losses
);