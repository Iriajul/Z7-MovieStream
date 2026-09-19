using System.Threading.RateLimiting;

namespace K7.Server.Web.Middleware;

/// <summary>
/// The Blazor SSR "/sign-in" page can't take <c>.RequireRateLimiting(...)</c> the way the
/// minimal API auth endpoints do (Connect/Token, GuestLogin, CompleteSetup), since it's a
/// razor component route, not an individually mappable endpoint. This applies the same
/// per-IP limit K7's AuthPolicy already uses elsewhere, but as middleware scoped to just
/// this one path and method, so the account-lockout in Identity isn't the only thing
/// slowing down repeated password guesses.
/// </summary>
public sealed class SignInRateLimitingMiddleware(RequestDelegate next)
{
    private static readonly PartitionedRateLimiter<HttpContext> Limiter =
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) || !context.Request.Path.StartsWithSegments("/sign-in"))
        {
            await next(context);
            return;
        }

        using var lease = await Limiter.AcquireAsync(context, 1, context.RequestAborted);
        if (!lease.IsAcquired)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }

        await next(context);
    }
}
