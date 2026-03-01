using System.Runtime.Serialization;

namespace Statikk_Data.ENUMs;

public enum EndOfGameResult : byte
{
    [EnumMember(Value = "GameComplete")]
    Completed,
    [EnumMember(Value = "Abort_Unexpected")]
    Unexpected,
    [EnumMember(Value = "Abort_TooFewPlayers")]
    TooFewPlayers,
    [EnumMember(Value = "Abort_AntiCheatExit")]
    AntiCheat
}