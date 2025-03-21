using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Movies;

public static class GetMovieEndpoint
{
    public const string Name = "GetMovie";

    public static IEndpointRouteBuilder MapGetMovie(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.Get,
                async (string idOrSlug, IMovieService movieService,
                    HttpContext context, CancellationToken cancellationToken) =>
                {
                    var userId = context.GetUserId();

                    var movie = Guid.TryParse(idOrSlug, out var id)
                        ? await movieService.GetByIdAsync(id, userId, cancellationToken)
                        : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

                    if (movie is null) return Results.NotFound();

                    var response = movie.MapToResponse();
                    return TypedResults.Ok(response);
                })
            .WithName(Name)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0)
            .CacheOutput("movies");
        return app;
    }
}