using Movies.Api.Auth;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Ratings;

public static class RateMovieEndpoint
{
    public const string Name = "RateMovie";

    public static IEndpointRouteBuilder MapRateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Rate, async (
                Guid id, RateMovieRequest request, IRatingService ratingService,
                HttpContext context, CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var result = await ratingService.RateMovieAsync(
                    id, request.Rating, userId!.Value, cancellationToken);
                return result ? Results.NoContent() : Results.NotFound();
            }).WithName(Name)
            .RequireAuthorization()
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0);
        return app;
    }
}