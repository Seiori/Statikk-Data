using System.Text.Json.Serialization;

namespace Statikk_Data;

[JsonSerializable(typeof(DTOs.LeagueEntry[]))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(DTOs.Match))]
public partial class RiotApiJsonContext : JsonSerializerContext { }