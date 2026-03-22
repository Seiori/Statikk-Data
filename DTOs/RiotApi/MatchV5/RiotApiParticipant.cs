using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiParticipant(
    [property: JsonPropertyName("puuid")] string Puuid,
    [property: JsonPropertyName("riotIdGameName")] string GameName,
    [property: JsonPropertyName("riotIdTagline")] string TagLine,
    [property: JsonPropertyName("profileIcon")] short IconId,
    [property: JsonPropertyName("summonerLevel")] short Level,
    [property: JsonPropertyName("teamPosition")] string Role,
    [property: JsonPropertyName("teamId")] short TeamId,
    [property: JsonPropertyName("championId")] short ChampionId,
    [property: JsonPropertyName("champLevel")] short ChampionLevel,
    [property: JsonPropertyName("perks")] RiotApiPerks Perks,
    [property: JsonPropertyName("summoner1Id")] short Spell1Id,
    [property: JsonPropertyName("summoner2Id")] short Spell2Id,
    [property: JsonPropertyName("kills")] short Kills,
    [property: JsonPropertyName("deaths")] short Deaths,
    [property: JsonPropertyName("assists")] short Assists,
    [property: JsonPropertyName("item0")] short Item1Id,
    [property: JsonPropertyName("item1")] short Item2Id,
    [property: JsonPropertyName("item2")] short Item3Id,
    [property: JsonPropertyName("item3")] short Item4Id,
    [property: JsonPropertyName("item4")] short Item5Id,
    [property: JsonPropertyName("item5")] short Item6Id,
    [property: JsonPropertyName("item6")] short WardId,
    [property: JsonPropertyName("gameEndedInEarlySurrender")] bool GameEndedInEarlySurrender,
    [property: JsonPropertyName("win")] bool Won
);