using System.ComponentModel;

namespace Statikk_Data.DTOs.Assets;

[ImmutableObject(true)]
public readonly record struct Asset(
    long Id,
    string Name,
    string ImageUrl
);