using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.AccountV1;

public readonly record struct RiotApiAccountDto(
    [property: JsonPropertyName("puuid")] string Puuid,
    [property: JsonPropertyName("gameName")] string GameName,
    [property: JsonPropertyName("tagLine")] string TagLine
);