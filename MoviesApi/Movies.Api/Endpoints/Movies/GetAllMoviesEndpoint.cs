using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies;

public static class GetAllMoviesEndpoint
{
    public const string Name = "GetMovies";

    public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll,
                async ([AsParameters] GetAllMoviesRequest request, IMovieService movieService,
                    HttpContext context, CancellationToken cancellationToken) =>
                {
                    var userId = context.GetUserId();

                    var options = request.MapToOptions();
                    var movies = await movieService.GetAllAsync(options, cancellationToken);
                    var movieCount = await movieService.GetCountAsync(options, cancellationToken);

                    var response = movies.MapToResponse(
                        request.Page.GetValueOrDefault(PagedRequest.DefaultPage),
                        request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize),
                        movieCount);
                    return TypedResults.Ok(response);
                })
            .WithName(Name)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0)
            .CacheOutput("movies");
        return app;
    }
}