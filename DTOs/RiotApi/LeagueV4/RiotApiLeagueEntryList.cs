using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.LeagueV4;

public readonly record struct RiotApiLeagueEntryList(
    [property: JsonPropertyName("entries")] RiotApiLeagueEntry[] LeagueEntries
);