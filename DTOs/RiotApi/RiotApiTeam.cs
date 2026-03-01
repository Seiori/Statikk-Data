using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi;

public readonly record struct RiotApiTeam(
    [property: JsonPropertyName("teamId")] short TeamId,
    [property: JsonPropertyName("win")] bool Win,
    [property: JsonPropertyName("bans")] RiotApiBan[] Bans,
    [property: JsonPropertyName("objectives")] RiotApiObjectives Objectives
);