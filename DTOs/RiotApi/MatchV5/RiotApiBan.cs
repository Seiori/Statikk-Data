using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiBan(
    [property: JsonPropertyName("championId")] short ChampionId,
    [property: JsonPropertyName("pickTurn")] short PickTurn
);