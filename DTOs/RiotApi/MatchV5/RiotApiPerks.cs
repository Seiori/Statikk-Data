using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiPerks(
    [property: JsonPropertyName("styles")] RiotApiPerkStyle[] Styles,
    [property: JsonPropertyName("statPerks")] RiotApiPerkStats Stats
);