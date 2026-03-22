using System.Text.Json.Serialization;

namespace Statikk_Data.DTOs.RiotApi.MatchV5;

public readonly record struct RiotApiObjectives(
    [property: JsonPropertyName("atakhan")] RiotApiObjectiveDetails Atakhan,
    [property: JsonPropertyName("baron")] RiotApiObjectiveDetails Baron,
    [property: JsonPropertyName("champion")] RiotApiObjectiveDetails Champion,
    [property: JsonPropertyName("dragon")] RiotApiObjectiveDetails Dragon,
    [property: JsonPropertyName("horde")] RiotApiObjectiveDetails Horde,
    [property: JsonPropertyName("inhibitor")] RiotApiObjectiveDetails Inhibitor,
    [property: JsonPropertyName("riftHerald")] RiotApiObjectiveDetails RiftHerald,
    [property: JsonPropertyName("tower")] RiotApiObjectiveDetails Tower
);