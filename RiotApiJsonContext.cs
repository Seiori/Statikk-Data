using System.Text.Json.Serialization;
using Statikk_Data.DTOs.RiotApi;
using Statikk_Data.DTOs.RiotApi.LeagueV4;

namespace Statikk_Data;

[JsonSerializable(typeof(RiotApiAccountDto))]
[JsonSerializable(typeof(RiotApiAccountRegionDto))]
[JsonSerializable(typeof(RiotApiLeagueEntryList))]
[JsonSerializable(typeof(RiotApiLeagueEntry[]))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(RiotApiMatch))]
[JsonSerializable(typeof(RiotApiSummonerDto))]
public partial class RiotApiJsonContext : JsonSerializerContext { }