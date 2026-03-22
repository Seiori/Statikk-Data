namespace Statikk_Data.DTOs.Assets;

public readonly record struct Asset(
    long Id,
    string Name,
    string ImageUrl
);