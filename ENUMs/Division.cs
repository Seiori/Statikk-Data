using System.Runtime.Serialization;

namespace Statikk_Data.ENUMs;

public enum Division : byte
{
    [EnumMember(Value = "IV")]
    Four = 4,
    [EnumMember(Value = "III")]
    Three = 3,
    [EnumMember(Value = "II")]
    Two = 2,
    [EnumMember(Value = "I")]
    One = 1
}