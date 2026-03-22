using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.AccountV1;

public readonly record struct RiotApiAccountRegionDto(
    [property: JsonPropertyName("region")] string Platform
);