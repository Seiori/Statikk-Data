using System.Collections.Frozen;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Statikk_Data.DTOs.Assets;
using Statikk_Data.ENUMs;
using Statikk_Data.Helpers;

namespace Statikk_Data.Workers;

public class StatikkData(
    IHttpClientFactory httpClientFactory, 
    ILogger<StatikkData> logger
) : BackgroundService
{
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(30);
    
    public Task WaitForInitialLoadAsync(
        CancellationToken cancellationToken = default
    ) 
    {
        return _initialLoadComplete.Task.WaitAsync(cancellationToken);
    }

    private static readonly Asset Fallback = new(-1, "Unknown", string.Empty);

    private const string CdnUrl = "https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default";
    private const string BaseDataUrl = $"{CdnUrl}/v1";
    private const string PatchVersionsUrl = "https://ddragon.leagueoflegends.com/api/versions.json";
    
    private volatile AssetSnapshot _snapshot = AssetSnapshot.Empty;
    private volatile IReadOnlyList<string> _patchVersions = [];
    private volatile IReadOnlyList<short> _patchIds = [];
    private readonly TaskCompletionSource _initialLoadComplete = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IReadOnlyList<string> PatchVersions => _patchVersions;
    public IReadOnlyList<short> PatchIds => _patchIds;

    public IReadOnlyDictionary<long, Asset> Champions => _snapshot.Champions;
    public IReadOnlyDictionary<long, Asset> Icons => _snapshot.Icons;
    public IReadOnlyDictionary<long, Asset> Items => _snapshot.Items;
    public IReadOnlyDictionary<long, Asset> Spells => _snapshot.Spells;
    public IReadOnlyDictionary<long, Asset> Runes => _snapshot.Runes;
    public IReadOnlyDictionary<long, Asset> RunePaths => _snapshot.RunePaths;

    public IEnumerable<Asset> AllChampions => Champions.Values;
    public IEnumerable<Asset> AllIcons => Icons.Values;
    public IEnumerable<Asset> AllItems => Items.Values;
    public IEnumerable<Asset> AllSpells => Spells.Values;
    public IEnumerable<Asset> AllRunes => Runes.Values;
    public IEnumerable<Asset> AllRunePaths => RunePaths.Values;

    private static readonly FrozenDictionary<Tier, string> TierUrlCache;

    static StatikkData()
    {
        TierUrlCache = TierExtensions.GetValues().ToArray()
            .ToFrozenDictionary(t => t, t => $"https://raw.communitydragon.org/latest/plugins/rcp-fe-lol-shared-components/global/default/images/{t.GetStringLowerCase()}.png");
    }

    public static string GetImageUrl(string path) => FormatUrl(path);
    public static string GetTierImageUrl(Tier t) => TierUrlCache.GetValueOrDefault(t, string.Empty);

    public Asset NoneChampion => Champions.GetValueOrDefault(-1, Fallback);
    public Asset GetChampion(long id) => Champions.GetValueOrDefault(id, NoneChampion);
    public Asset GetItem(long id) => Items.GetValueOrDefault(id, Fallback);
    public Asset GetIcon(long id) => Icons.GetValueOrDefault(id, Fallback);
    public Asset GetRune(long id) => Runes.GetValueOrDefault(id, Fallback);
    public Asset GetRunePath(long id) => RunePaths.GetValueOrDefault(id, Fallback);
    public Asset GetSpell(long id) => Spells.GetValueOrDefault(id, Fallback);

    protected override async Task ExecuteAsync(
        CancellationToken ct
    )
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync();
                _initialLoadComplete.TrySetResult();
                logger.LogInformation("Assets refreshed.");
            }
            catch (Exception ex) { logger.LogError(ex, "Refresh failed."); }
            
            try
            {
                await Task.Delay(_refreshInterval, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RefreshAsync()
    {
        using var client = httpClientFactory.CreateClient("CommunityDragon");

        var championsTask = Fetch(client, $"{BaseDataUrl}/champion-summary.json");
        var iconsTask = Fetch(client, $"{BaseDataUrl}/summoner-icons.json");
        var itemsTask = Fetch(client, $"{BaseDataUrl}/items.json");
        var spellsTask = Fetch(client, $"{BaseDataUrl}/summoner-spells.json");
        var runesTask = Fetch(client, $"{BaseDataUrl}/perks.json");
        var runePathsTask = FetchRunePaths(client);
        var patchVersionsTask = FetchPatchVersions(client);

        await Task.WhenAll(
            championsTask,
            iconsTask,
            itemsTask,
            spellsTask,
            runesTask,
            runePathsTask,
            patchVersionsTask
        );

        _snapshot = new AssetSnapshot(
            championsTask.Result,
            iconsTask.Result,
            itemsTask.Result,
            spellsTask.Result,
            runesTask.Result,
            runePathsTask.Result
        );

        var patchVersions = patchVersionsTask.Result;
        _patchVersions = patchVersions;
        _patchIds = patchVersions.Select(Utilities.ToPatchId).ToList();
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

    private static async Task<IReadOnlyList<string>> FetchPatchVersions(HttpClient c)
    {
        var versions = await c.GetFromJsonAsync<string[]>(PatchVersionsUrl);
        if (versions is null || versions.Length == 0) return [];

        return versions
            .Select(v =>
            {
                var parts = v.Split('.');
                return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : null;
            })
            .Where(v => v is not null)
            .Distinct()
            .Take(3)
            .ToList()!;
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
        FrozenDictionary<long, Asset> RunePaths
    )
    {
        public static readonly AssetSnapshot Empty = new(
            FrozenDictionary<long, Asset>.Empty, 
            FrozenDictionary<long, Asset>.Empty, 
            FrozenDictionary<long, Asset>.Empty, 
            FrozenDictionary<long, Asset>.Empty, 
            FrozenDictionary<long, Asset>.Empty, 
            FrozenDictionary<long, Asset>.Empty
        );
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(Asset[]))]
internal partial class AssetJsonContext : JsonSerializerContext;