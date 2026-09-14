using K7.Server.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace K7.Server.Web.Endpoints.TrendingPosters;

public class GetTrendingPosters : IEndpoint
{
    public void Map(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var type = GetType();
        string groupName = type.Namespace!.Split('.').Last();

        endpointRouteBuilder.MapGet("/api/trending-posters", async (
            [FromServices] TrendingPosterService posters,
            CancellationToken cancellationToken) => await posters.GetShuffledAsync(cancellationToken))
        .AllowAnonymous()
        .WithName(type.Name)
        .WithTags(groupName);
    }
}
