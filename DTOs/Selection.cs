using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs;

public readonly record struct Selection(
    [property: JsonPropertyName("perk")] short Perk    
);