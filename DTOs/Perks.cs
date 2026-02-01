using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs;

public readonly record struct Perks(
    [property: JsonPropertyName("styles")] PerkStyle[] Styles,
    [property: JsonPropertyName("statPerks")] PerkStats Stats
);