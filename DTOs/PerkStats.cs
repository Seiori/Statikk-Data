using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs;

public readonly record struct PerkStats(
    [property: JsonPropertyName("offense")] short Offense,
    [property: JsonPropertyName("defense")] short Defense,
    [property: JsonPropertyName("flex")] short Flex
);