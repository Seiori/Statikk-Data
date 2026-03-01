using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi;

public readonly record struct RiotApiSelection(
    [property: JsonPropertyName("perk")] short Perk    
);