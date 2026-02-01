using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs;

public readonly record struct PerkStyle(
    [property: JsonPropertyName("selections")] Selection[] Selections,
    [property: JsonPropertyName("style")] short Style
);