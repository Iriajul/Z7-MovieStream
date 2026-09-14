using Microsoft.Extensions.Caching.Memory;
using TMDbLib.Client;
using TMDbLib.Objects.Trending;

namespace K7.Server.Web.Infrastructure;

/// <summary>Decorative poster URLs of this week's trending movies on TMDb (not library content).</summary>
public sealed class TrendingPosterService(TMDbClient tmdb, IMemoryCache cache, ILogger<TrendingPosterService> logger)
{
    private const string CacheKey = "trending-posters";

    public async Task<IReadOnlyList<string>> GetShuffledAsync(CancellationToken cancellationToken = default)
    {
        if (!cache.TryGetValue(CacheKey, out List<string>? urls) || urls is null)
        {
            urls = [];
            try
            {
                for (var page = 1; page <= 2; page++)
                {
                    var trending = await tmdb.GetTrendingMoviesAsync(TimeWindow.Week, page, cancellationToken: cancellationToken);
                    urls.AddRange((trending?.Results ?? [])
                        .Where(m => !string.IsNullOrEmpty(m.PosterPath))
                        .Select(m => $"https://image.tmdb.org/t/p/w342{m.PosterPath}"));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Could not load trending posters from TMDb");
            }

            // Cache failures briefly so an offline server doesn't retry on every visit.
            cache.Set(CacheKey, urls, urls.Count > 0 ? TimeSpan.FromHours(6) : TimeSpan.FromMinutes(5));
        }

        return urls.OrderBy(_ => Random.Shared.Next()).ToList();
    }
}
