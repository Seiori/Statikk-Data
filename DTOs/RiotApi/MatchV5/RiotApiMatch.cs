using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiMatch(
    [property: JsonPropertyName("info")] RiotApiInfo Info
);