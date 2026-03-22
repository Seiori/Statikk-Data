using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiInfo(
    [property: JsonPropertyName("endOfGameResult")] string EndOfGameResult,
    [property: JsonPropertyName("gameStartTimestamp")] long StartedAt,
    [property: JsonPropertyName("gameDuration")] short Duration,
    [property: JsonPropertyName("gameId")] long GameId,
    [property: JsonPropertyName("gameVersion")] string PatchVersion,
    [property: JsonPropertyName("participants")] RiotApiParticipant[] Participants,
    [property: JsonPropertyName("platformId")] string Platform,
    [property: JsonPropertyName("teams")] RiotApiTeam[] Teams
);