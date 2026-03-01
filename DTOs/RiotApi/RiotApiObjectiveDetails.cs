using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi;

public readonly record struct RiotApiObjectiveDetails(
    [property: JsonPropertyName("first")] bool First,
    [property: JsonPropertyName("kills")] short Kills
);