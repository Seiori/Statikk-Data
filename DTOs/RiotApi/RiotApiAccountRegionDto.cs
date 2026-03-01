using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi;

public readonly record struct RiotApiAccountRegionDto(
    [property: JsonPropertyName("region")] string Platform
);