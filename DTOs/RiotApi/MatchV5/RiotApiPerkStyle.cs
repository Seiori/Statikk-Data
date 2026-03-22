using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiPerkStyle(
    [property: JsonPropertyName("selections")] RiotApiSelection[] Selections,
    [property: JsonPropertyName("style")] short Style
);