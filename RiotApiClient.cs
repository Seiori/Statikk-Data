using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;
using Statikk_Data.Endpoints;
using Statikk_Data.ENUMs;

namespace Statikk_Data;

public sealed class RiotApiClient
{
    private readonly HttpClient _httpClient;
    private readonly RiotApiRouteManager _routeManager;

    public AccountV1 Account { get; }
    public LeagueV4 League { get; }
    public MatchV5 MatchV5 { get; }
    public SummonerV4 SummonerV4 { get; }
    
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
        
        Account = new AccountV1(this);
        League = new LeagueV4(this);
        MatchV5 = new MatchV5(this);
        SummonerV4 = new SummonerV4(this);
    }
    
    public async Task<T?> SendAsync<T>(
        RegionalRoute regionalRoute,
        Methods method, 
        string url, 
        JsonTypeInfo<T> typeInfo,
        CancellationToken ct
    )
    {
        var routeRateLimiter = _routeManager.RegionalRateLimiters[regionalRoute];
        var routeMethodRateLimiter = _routeManager.RegionalMethodRateLimiters[(regionalRoute, method)];
        
        return await SendInternalAsync(routeRateLimiter, routeMethodRateLimiter, url, typeInfo, ct);
    }

    public async Task<T?> SendAsync<T>(
        PlatformRoute platformRoute,
        Methods method,
        string url,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken
    )
    {
        var routeRateLimiter = _routeManager.PlatformRateLimiters[platformRoute];
        var routeMethodRateLimiter = _routeManager.PlatformMethodRateLimiters[(platformRoute, method)];
        
        return await SendInternalAsync(routeRateLimiter, routeMethodRateLimiter, url, typeInfo, cancellationToken);
    }

    private async Task<T?> SendInternalAsync<T>(
        RateLimiter routeRateLimiter,
        RateLimiter routeMethodRateLimiter,
        string url, 
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken
    )
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await routeRateLimiter.WaitAsync(cancellationToken);
                await routeMethodRateLimiter.WaitAsync(cancellationToken);

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                await SyncAll(routeRateLimiter, routeMethodRateLimiter, response.Headers);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    
                    return await System.Text.Json.JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken);
                }
                
                switch (response.StatusCode)
                {
                    case HttpStatusCode.TooManyRequests:
                    {
                        var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(attempt);
                        
                        await Task.Delay(retryAfter, cancellationToken);
                        
                        continue;
                    }
                    case HttpStatusCode.NotFound:
                        return default;
                    default:
                        response.EnsureSuccessStatusCode();
                        break;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        return default;
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
    
        if (!h.NonValidated.TryGetValues(lKey, out var lVal) || 
            !h.NonValidated.TryGetValues(cKey, out var cVal)) 
            return false;

        lSeg = GetFirstFromHeaderValues(lVal);
        cSeg = GetFirstFromHeaderValues(cVal);
        return true;

        static ReadOnlySpan<char> GetFirstFromHeaderValues(HeaderStringValues values)
        {
            foreach (var val in values)
            {
                var span = val.AsSpan();
                var idx = span.IndexOf(',');
                return idx == -1 ? span : span[..idx];
            }
            return default;
        }
    }

    private static (int Limit, TimeSpan Window, int Count) ParseRiotHeader(ReadOnlySpan<char> l, ReadOnlySpan<char> c)
    {
        var lIdx = l.IndexOf(':');
        var cIdx = c.IndexOf(':');
        return (int.Parse(l[..lIdx]), TimeSpan.FromSeconds(int.Parse(l[(lIdx + 1)..])), int.Parse(c[..cIdx]));
    }
}