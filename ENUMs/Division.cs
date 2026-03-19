using System.Text.Json.Serialization;

namespace Statikk_Data.ENUMs;

public enum Division : byte
{
    None = 0,
    [JsonPropertyName("I")]
    One = 1,
    [JsonPropertyName("II")]
    Two = 2,
    [JsonPropertyName("III")]
    Three = 3,
    [JsonPropertyName("IV")]
    Four = 4,
}