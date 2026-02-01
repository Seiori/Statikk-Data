using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using Statikk_Data.Endpoints;
using Statikk_Data.ENUMs;

namespace Statikk_Data;

public sealed class RiotApiClient
{
    private readonly HttpClient _httpClient;
    private readonly RiotApiRouteManager _routeManager;

    public LeagueExpV4 LeagueExp { get; }
    public MatchV5 MatchV5 { get; }
    
    private const string AppLimitHeader = "X-App-Rate-Limit";
    private const string AppCountHeader = "X-App-Rate-Limit-Count";
    private const string MethodLimitHeader = "X-Method-Rate-Limit";
    private const string MethodCountHeader = "X-Method-Rate-Limit-Count";

    public RiotApiClient(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
        _routeManager = new RiotApiRouteManager();
        
        LeagueExp = new LeagueExpV4(this);
        MatchV5 = new MatchV5(this);
    }
    
    public async Task<T?> SendAsync<T>(
        RegionalRoute regionalRoute, 
        string method, 
        string endpoint, 
        CancellationToken ct
    )
    {
        var regionalRouteStr = regionalRoute.GetStringLowerCase();
        var baseUrl = RiotApi.GetUrl(regionalRoute);
        var url = $"{baseUrl}/{endpoint}";
        return await SendInternalAsync<T>(regionalRouteStr, method, url, ct);
    }

    public async Task<T?> SendAsync<T>(
        PlatformRoute platformRoute,
        string method,
        string endpoint,
        CancellationToken cancellationToken
    )
    {
        var platformRouteStr = platformRoute.GetStringLowerCase();
        var baseUrl = RiotApi.GetUrl(platformRoute);
        var url = $"{baseUrl}/{endpoint}";
        return await SendInternalAsync<T>(platformRouteStr, method, url, cancellationToken);
    }

    private async Task<T?> SendInternalAsync<T>(
        string route, 
        string method, 
        string url, 
        CancellationToken cancellationToken
    )
    {
        var appRateLimiter = _routeManager.AppRateLimiters.GetOrAdd(route, _ => new RateLimiter());
        var methodRateLimiter = _routeManager.MethodRateLimiters.GetOrAdd((route, method), _ => new RateLimiter());

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await appRateLimiter.WaitAsync(cancellationToken);
            await methodRateLimiter.WaitAsync(cancellationToken);

            using var response = await _httpClient.GetAsync(url, cancellationToken);

            await SyncAll(appRateLimiter, methodRateLimiter, response.Headers);

            if (response.StatusCode is HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5);
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            response.EnsureSuccessStatusCode();
        
            return await response.Content.ReadFromJsonAsync((JsonTypeInfo<T>)RiotApiJsonContext.Default.GetTypeInfo(typeof(T))!, cancellationToken);
        }
        
        throw new Exception("Failed to send request after multiple attempts due to rate limiting.");
    }

    private static async ValueTask SyncAll(
        RateLimiter appRateLimiter, 
        RateLimiter methodRateLimiter, 
        HttpResponseHeaders headers
    )
    {
        if (TryGetSegments(headers, AppLimitHeader, AppCountHeader, out var appL, out var appC))
        {
            var (limit, window, count) = ParseRiotHeader(appL, appC);
            await appRateLimiter.SyncAsync(limit, window, count);
        }

        if (TryGetSegments(headers, MethodLimitHeader, MethodCountHeader, out var methL, out var methC))
        {
            var (limit, window, count) = ParseRiotHeader(methL, methC);
            await methodRateLimiter.SyncAsync(limit, window, count);
        }
    }

    private static bool TryGetSegments(HttpResponseHeaders h, string lKey, string cKey, out ReadOnlySpan<char> lSeg, out ReadOnlySpan<char> cSeg)
    {
        lSeg = default; cSeg = default;
        if (!h.TryGetValues(lKey, out var lVal) || !h.TryGetValues(cKey, out var cVal)) return false;

        lSeg = GetFirst(lVal.First());
        cSeg = GetFirst(cVal.First());
        return true;

        static ReadOnlySpan<char> GetFirst(string s)
        {
            var span = s.AsSpan();
            var idx = span.IndexOf(',');
            return idx == -1 ? span : span[..idx];
        }
    }

    private static (int Limit, TimeSpan Window, int Count) ParseRiotHeader(ReadOnlySpan<char> l, ReadOnlySpan<char> c)
    {
        var lIdx = l.IndexOf(':');
        var cIdx = c.IndexOf(':');
        return (int.Parse(l[..lIdx]), TimeSpan.FromSeconds(int.Parse(l[(lIdx + 1)..])), int.Parse(c[..cIdx]));
    }
}