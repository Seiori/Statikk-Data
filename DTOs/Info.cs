using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs;

public readonly record struct Info(
    [property: JsonPropertyName("gameVersion")] string PatchVersion,
    [property: JsonPropertyName("participants")] Participant[] Participants,
    [property: JsonPropertyName("gameId")] long GameId,
    [property: JsonPropertyName("gameCreation")] long StartedAt,
    [property: JsonPropertyName("gameDuration")] short Duration,
    [property: JsonPropertyName("queueId")] short QueueId
);