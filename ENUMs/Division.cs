using System.Text.Json.Serialization;

namespace Statikk_Data.ENUMs;

public enum Division : byte
{
    None,
    [JsonPropertyName("I")]
    One,
    [JsonPropertyName("II")]
    Two,
    [JsonPropertyName("III")]
    Three,
    [JsonPropertyName("IV")]
    Four,
}