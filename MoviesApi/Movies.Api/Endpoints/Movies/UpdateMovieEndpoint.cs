using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies;

public static class UpdateMovieEndpoint
{
    public const string Name = "UpdateMovie";

    public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update, async (
                Guid id, UpdateMovieRequest request, IMovieService movieService,
                IOutputCacheStore outputCacheStore, HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var userid = context.GetUserId();
                var movie = request.MapToMovie();
                var updatedMovie = await movieService.UpdateAsync(movie, userid, cancellationToken);
                if (updatedMovie is null) return Results.NotFound();
                await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
                return TypedResults.Ok(updatedMovie.MapToResponse());
            }).WithName(Name)
            .RequireAuthorization(AuthConstants.TrustedMemberPolicyName)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0);
        return app;
    }
}