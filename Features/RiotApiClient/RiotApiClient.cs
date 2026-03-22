using System.Collections.Frozen;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization.Metadata;
using Statikk_Data.DTOs.RiotApi.RiotApiClient;
using Statikk_Data.Endpoints;
using Statikk_Data.ENUMs;

namespace Statikk_Data.Features.RiotApiClient;

public sealed class RiotApiClient
{
    private readonly HttpClient _httpClient;

    private readonly FrozenDictionary<RegionalRoute, RateLimiter.RateLimiter> _regionalRouteRateLimiters;
    private readonly FrozenDictionary<(RegionalRoute, Methods), RateLimiter.RateLimiter> _regionalRouteMethodRateLimiters;
    
    private readonly FrozenDictionary<PlatformRoute, RateLimiter.RateLimiter> _platformRouteRateLimiters;
    private readonly FrozenDictionary<(PlatformRoute, Methods), RateLimiter.RateLimiter> _platformRouteMethodRateLimiters;
    
    public AccountV1 Account { get; }
    public LeagueExpV4 LeagueExp { get; }
    public LeagueV4 League { get; }
    public MatchV5 MatchV5 { get; }
    
    private const string MethodLimitHeader = "X-Method-Rate-Limit";

    public RiotApiClient(
        HttpClient httpClient,
        RiotApiClientSettings settings
    )
    {
        _httpClient = httpClient;
        
        var regionalRateLimiters = new Dictionary<RegionalRoute, RateLimiter.RateLimiter>(RegionalRouteExtensions.Length);
        var regionalMethodRateLimiters = new Dictionary<(RegionalRoute, Methods), RateLimiter.RateLimiter>(RegionalRouteExtensions.Length * MethodsExtensions.Length);
        
        foreach (var regionalRoute in RiotApi.RegionalRoutes.Where(r => r is not RegionalRoute.None))
        {
            regionalRateLimiters[regionalRoute] = new RateLimiter.RateLimiter(settings.Requests, TimeSpan.FromSeconds(settings.Seconds));
            
            foreach (var method in MethodsExtensions.GetValues())
            {
                regionalMethodRateLimiters[(regionalRoute, method)] = new RateLimiter.RateLimiter();
            }
        }
        
        _regionalRouteRateLimiters = regionalRateLimiters.ToFrozenDictionary();
        _regionalRouteMethodRateLimiters = regionalMethodRateLimiters.ToFrozenDictionary();

        var platformRateLimiters = new Dictionary<PlatformRoute, RateLimiter.RateLimiter>(PlatformRouteExtensions.Length);
        var platformMethodRateLimiters = new Dictionary<(PlatformRoute, Methods), RateLimiter.RateLimiter>(PlatformRouteExtensions.Length * MethodsExtensions.Length);
        
        foreach (var platformRoute in RiotApi.PlatformRoutes.Where(p => p is not PlatformRoute.None))
        {
            platformRateLimiters[platformRoute] = new RateLimiter.RateLimiter(settings.Requests, TimeSpan.FromSeconds(settings.Seconds));
            
            foreach (var method in MethodsExtensions.GetValues())
            {
                platformMethodRateLimiters[(platformRoute, method)] = new RateLimiter.RateLimiter();
            }
        }

        _platformRouteRateLimiters = platformRateLimiters.ToFrozenDictionary();
        _platformRouteMethodRateLimiters = platformMethodRateLimiters.ToFrozenDictionary();
        
        Account = new AccountV1(this);
        LeagueExp = new LeagueExpV4(this);
        League = new LeagueV4(this);
        MatchV5 = new MatchV5(this);
    }
    
    public async Task<T?> SendAsync<T>(
        RegionalRoute regionalRoute,
        Methods method, 
        string url, 
        JsonTypeInfo<T> typeInfo,
        CancellationToken ct
    )
    {
        return await SendInternalAsync(
            _regionalRouteRateLimiters[regionalRoute],
            _regionalRouteMethodRateLimiters[(regionalRoute, method)],
            url, 
            typeInfo, 
            ct
        ).ConfigureAwait(false);
    }

    public async Task<T?> SendAsync<T>(
        PlatformRoute platformRoute,
        Methods method,
        string url,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken
    )
    {
        return await SendInternalAsync(
            _platformRouteRateLimiters[platformRoute], 
            _platformRouteMethodRateLimiters[(platformRoute, method)], 
            url, 
            typeInfo, 
            cancellationToken
        ).ConfigureAwait(false);
    }

    private async Task<T?> SendInternalAsync<T>(
        RateLimiter.RateLimiter appRateLimiter,
        RateLimiter.RateLimiter methodRateLimiter,
        string url, 
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken
    )
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await appRateLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
                await methodRateLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

                if (TryGetMethodLimits(response.Headers, out var methLimit, out var methWindow))
                {
                    methodRateLimiter.TryInitialize(methLimit, methWindow);
                }

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    return await System.Text.Json.JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken).ConfigureAwait(false);
                }
                
                switch (response.StatusCode)
                {
                    case HttpStatusCode.TooManyRequests:
                    {
                        var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(attempt);
                        await Task.Delay(retryAfter, cancellationToken).ConfigureAwait(false);
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
                await Task.Delay(
                    CalculateExponentialBackoff(attempt), 
                    cancellationToken
                ).ConfigureAwait(false);
            }
        }

        return default;
    }
    
    private static bool TryGetMethodLimits(
        HttpResponseHeaders headers, 
        out int limit, 
        out TimeSpan window
    )
    {
        limit = 0;
        window = TimeSpan.Zero;

        if (!headers.NonValidated.TryGetValues(MethodLimitHeader, out var limitVals))
        {
            return false;
        }

        foreach (var val in limitVals)
        {
            var span = val.AsSpan();
            var commaIdx = span.IndexOf(',');
            var segment = commaIdx == -1 ? span : span[..commaIdx];
            
            var colonIdx = segment.IndexOf(':');
            if (colonIdx != -1)
            {
                limit = int.Parse(segment[..colonIdx]);
                window = TimeSpan.FromSeconds(int.Parse(segment[(colonIdx + 1)..]));
                return true;
            }
        }

        return false;
    }
    
    private static TimeSpan CalculateExponentialBackoff(
        int attempt
    )
    {
        var baseDelay = TimeSpan.FromSeconds(1);
        var exponentialMultiplier = Math.Pow(2, attempt);
        var jitter = Random.Shared.NextDouble() * 1000;
    
        var totalMilliseconds = (baseDelay.TotalMilliseconds * exponentialMultiplier) + jitter;
    
        return TimeSpan.FromMilliseconds(totalMilliseconds);
    }
}