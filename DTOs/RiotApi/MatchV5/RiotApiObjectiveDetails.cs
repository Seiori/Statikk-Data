using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiObjectiveDetails(
    [property: JsonPropertyName("first")] bool First,
    [property: JsonPropertyName("kills")] short Kills
);