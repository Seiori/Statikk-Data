using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiSelection(
    [property: JsonPropertyName("perk")] short Perk    
);