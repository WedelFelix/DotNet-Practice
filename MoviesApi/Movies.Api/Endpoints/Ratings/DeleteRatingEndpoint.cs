using Movies.Api.Auth;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Ratings;

public static class DeleteMovieEndpoint
{
    public const string Name = "DeleteRating";

    public static IEndpointRouteBuilder MapDeleteRating(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.DeleteRating, async (
                Guid id, IRatingService ratingService,
                HttpContext context, CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var result = await ratingService.DeleteRatingAsync(
                    id, userId!.Value, cancellationToken);
                return result ? Results.NoContent() : Results.NotFound();
            }).WithName(Name)
            .RequireAuthorization()
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0);
        return app;
    }
}