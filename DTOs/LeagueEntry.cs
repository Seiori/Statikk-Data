using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs;

public readonly record struct LeagueEntry(
    [property: JsonPropertyName("puuid")] string Puuid,
    [property: JsonPropertyName("tier")] string Tier,
    [property: JsonPropertyName("rank")] string Division,
    [property: JsonPropertyName("leaguePoints")] short LeaguePoints,
    [property: JsonPropertyName("wins")] short Wins,
    [property: JsonPropertyName("losses")] short Losses
);