using System.Collections.Frozen;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Statikk_Data.DTOs.Assets;
using Statikk_Data.ENUMs;

namespace Statikk_Data.Workers;

public class StatikkData(IHttpClientFactory httpClientFactory, ILogger<StatikkData> logger) : BackgroundService
{
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(30);
    private static readonly Asset Fallback = new(-1, "Unknown", string.Empty);

    private const string CdnUrl = "https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default";
    private const string BaseDataUrl = $"{CdnUrl}/v1";
    
    private volatile AssetSnapshot _s = AssetSnapshot.Empty;

    private static readonly FrozenDictionary<Tier, string> TierUrlCache;

    static StatikkData()
    {
        TierUrlCache = TierExtensions.GetValues().ToArray()
            .ToFrozenDictionary(t => t, t => $"https://raw.communitydragon.org/latest/plugins/rcp-fe-lol-shared-components/global/default/images/{t.GetStringLowerCase()}.png");
    }

    public static string GetImageUrl(string path) => FormatUrl(path);
    public static string GetTierImageUrl(Tier t) => TierUrlCache.GetValueOrDefault(t, string.Empty);

    public Asset NoneChampion => _s.Champions.GetValueOrDefault(-1, Fallback);
    public Asset GetChampion(long id) => _s.Champions.GetValueOrDefault(id, NoneChampion);
    public Asset GetItem(long id) => _s.Items.GetValueOrDefault(id, Fallback);
    public Asset GetIcon(long id) => _s.Icons.GetValueOrDefault(id, Fallback);
    public Asset GetRune(long id) => _s.Runes.GetValueOrDefault(id, Fallback);
    public Asset GetRunePath(long id) => _s.RunePaths.GetValueOrDefault(id, Fallback);
    public Asset GetSpell(long id) => _s.Spells.GetValueOrDefault(id, Fallback);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var client = httpClientFactory.CreateClient("CommunityDragon");
                var tasks = new[] {
                    Fetch(client, $"{BaseDataUrl}/champion-summary.json"),
                    Fetch(client, $"{BaseDataUrl}/summoner-icons.json"),
                    Fetch(client, $"{BaseDataUrl}/items.json"),
                    Fetch(client, $"{BaseDataUrl}/summoner-spells.json"),
                    Fetch(client, $"{BaseDataUrl}/perks.json"),
                    FetchRunePaths(client)
                };

                await Task.WhenAll(tasks);

                _s = new AssetSnapshot(tasks[0].Result, tasks[1].Result, tasks[2].Result, tasks[3].Result, tasks[4].Result, tasks[5].Result);
                logger.LogInformation("Assets refreshed.");
            }
            catch (Exception ex) { logger.LogError(ex, "Refresh failed."); }
            
            await Task.Delay(_refreshInterval, ct);
        }
    }

    private static async Task<FrozenDictionary<long, Asset>> Fetch(HttpClient c, string url)
    {
        var data = await c.GetFromJsonAsync(url, AssetJsonContext.Default.AssetArray);
        return data?.DistinctBy(x => x.Id).ToFrozenDictionary(x => x.Id) ?? FrozenDictionary<long, Asset>.Empty;
    }

    private static async Task<FrozenDictionary<long, Asset>> FetchRunePaths(HttpClient c)
    {
        using var doc = await JsonDocument.ParseAsync(await c.GetStreamAsync($"{BaseDataUrl}/perkstyles.json"));
        return doc.RootElement.TryGetProperty("styles", out var el) 
            ? el.Deserialize(AssetJsonContext.Default.AssetArray)?.ToFrozenDictionary(x => x.Id) ?? FrozenDictionary<long, Asset>.Empty 
            : FrozenDictionary<long, Asset>.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        const string prefix = "/lol-game-data/assets";
        var span = path.AsSpan();
        if (!span.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return path.ToLowerInvariant();

        return string.Create(CdnUrl.Length + (span.Length - prefix.Length), (path, prefix.Length), (dest, state) => {
            CdnUrl.AsSpan().CopyTo(dest);
            state.path.AsSpan(state.Length).ToLowerInvariant(dest[CdnUrl.Length..]);
        });
    }

    private record AssetSnapshot(
        FrozenDictionary<long, Asset> Champions,
        FrozenDictionary<long, Asset> Icons,
        FrozenDictionary<long, Asset> Items,
        FrozenDictionary<long, Asset> Spells,
        FrozenDictionary<long, Asset> Runes,
        FrozenDictionary<long, Asset> RunePaths)
    {
        public static readonly AssetSnapshot Empty = new(
            FrozenDictionary<long, Asset>.Empty, FrozenDictionary<long, Asset>.Empty, 
            FrozenDictionary<long, Asset>.Empty, FrozenDictionary<long, Asset>.Empty, 
            FrozenDictionary<long, Asset>.Empty, FrozenDictionary<long, Asset>.Empty);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(Asset[]))]
internal partial class AssetJsonContext : JsonSerializerContext;