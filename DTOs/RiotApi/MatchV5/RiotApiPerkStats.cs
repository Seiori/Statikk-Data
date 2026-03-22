using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiPerkStats(
    [property: JsonPropertyName("offense")] short Offense,
    [property: JsonPropertyName("defense")] short Defense,
    [property: JsonPropertyName("flex")] short Flex
);