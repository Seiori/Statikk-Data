using System.Collections.Frozen;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Statikk_Data.DTOs.Assets;
using Statikk_Data.ENUMs;
using Statikk_Data.Features.RiotApiClient;
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
    
    private volatile string[] _patchVersions = [];
    private volatile short[] _patchIds = [];
    
    private readonly TaskCompletionSource _initialLoadComplete = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string[] PatchVersions => _patchVersions;
    public short[] PatchIds => _patchIds;

    public FrozenDictionary<long, Asset> Champions => _snapshot.Champions;
    public FrozenDictionary<long, Asset> Icons => _snapshot.Icons;
    public FrozenDictionary<long, Asset> Items => _snapshot.Items;
    public FrozenDictionary<long, Asset> Spells => _snapshot.Spells;
    public FrozenDictionary<long, Asset> Runes => _snapshot.Runes;
    public FrozenDictionary<long, Asset> RunePaths => _snapshot.RunePaths;

    private static readonly FrozenDictionary<Tier, string> TierUrlCache;

    static StatikkData()
    {
        TierUrlCache = RiotApi.Tiers
            .ToFrozenDictionary(
                tier => tier, 
                tier => $"https://raw.communitydragon.org/latest/plugins/rcp-fe-lol-shared-components/global/default/images/{tier.GetStringLowerCase()}.png"
            );
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
        CancellationToken cancellationToken
    )
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync().ConfigureAwait(false);
                
                _initialLoadComplete.TrySetResult();
                logger.LogInformation("Assets refreshed.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Assets refresh failed.");
            }
            
            try
            {
                await Task.Delay(_refreshInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RefreshAsync()
    {
        using var client = httpClientFactory.CreateClient("StatikkDataClient");

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
        ).ConfigureAwait(false);

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
        _patchIds = patchVersions.Select(Utilities.ToPatchId).ToArray();
    }

    private static async Task<FrozenDictionary<long, Asset>> Fetch(HttpClient c, string url)
    {
        var data = await c.GetFromJsonAsync(url, AssetJsonContext.Default.AssetArray).ConfigureAwait(false);
        return data?.DistinctBy(x => x.Id).ToFrozenDictionary(x => x.Id) ?? FrozenDictionary<long, Asset>.Empty;
    }

    private static async Task<FrozenDictionary<long, Asset>> FetchRunePaths(HttpClient c)
    {
        using var doc = await JsonDocument.ParseAsync(await c.GetStreamAsync($"{BaseDataUrl}/perkstyles.json")).ConfigureAwait(false);
        return doc.RootElement.TryGetProperty("styles", out var el) 
            ? el.Deserialize(AssetJsonContext.Default.AssetArray)?
                .Where(x => x.Id > 0)
                .DistinctBy(x => x.Id)
                .ToFrozenDictionary(x => x.Id) ?? FrozenDictionary<long, Asset>.Empty 
            : FrozenDictionary<long, Asset>.Empty;
    }

    private static async Task<string[]> FetchPatchVersions(HttpClient c)
    {
        var versions = await c.GetFromJsonAsync<string[]>(PatchVersionsUrl);
        if (versions is null || versions.Length == 0)
        {
            return [];
        }

        return versions
            .Select(v =>
            {
                var parts = v.Split('.');
                return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : null;
            })
            .Where(v => v is not null)
            .Distinct()
            .Take(3)
            .ToArray()!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }
        
        const string prefix = "/lol-game-data/assets";
        var span = path.AsSpan();
        if (!span.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return path.ToLowerInvariant();
        }

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

[JsonSerializable(typeof(Asset[]))]
internal partial class AssetJsonContext : JsonSerializerContext;