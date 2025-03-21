using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Movies;

public static class DeleteMovieEndpoint
{
    public const string Name = "DeleteMovie";

    public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Delete, async (
                Guid id, IMovieService movieService,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
            {
                var isDeleted = await movieService.DeleteByIdAsync(id, cancellationToken);
                if (!isDeleted) return Results.NotFound();
                await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
                return Results.NoContent();
            }).WithName(Name)
            .RequireAuthorization(AuthConstants.AdminUserPolicyName)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0);
        return app;
    }
}