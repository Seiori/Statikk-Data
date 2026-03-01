using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi;

public readonly record struct RiotApiSummonerDto(
    [property: JsonPropertyName("puuid")] string Puuid,
    [property: JsonPropertyName("profileIconId")] short IconId,
    [property: JsonPropertyName("summonerLevel")] short Level
);