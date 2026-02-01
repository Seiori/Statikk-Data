using System.Runtime.Serialization;

namespace Statikk_Data.ENUMs;

public enum Role : byte
{
    None = 0,
    Top = 1,
    Jungle = 2,
    Middle = 3,
    Bottom = 4,
    [EnumMember(Value = "UTILITY")]
    Support = 5
}