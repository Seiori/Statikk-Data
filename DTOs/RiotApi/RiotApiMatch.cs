using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi;

public readonly record struct RiotApiMatch(
    [property: JsonPropertyName("info")] RiotApiInfo Info
);