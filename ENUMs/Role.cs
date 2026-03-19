using System.Runtime.Serialization;

namespace Statikk_Data.ENUMs;

public enum Role : byte
{
    None,
    Top,
    Jungle,
    Middle,
    Bottom,
    [EnumMember(Value = "UTILITY")]
    Support
}