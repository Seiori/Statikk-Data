using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs;

public readonly record struct Match(
    [property: JsonPropertyName("info")] Info Info
);