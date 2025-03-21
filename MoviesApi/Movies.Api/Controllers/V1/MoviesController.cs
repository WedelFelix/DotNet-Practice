using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
public class MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore) : ControllerBase
{
    private readonly IMovieService _movieService = movieService;

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
    {
        request.MapToMovie();
        var movie = request.MapToMovie();

        // We should always be creating and passing down cancellation tokens from our controllers
        // This helps us save on resources when a request is cancelled
        await _movieService.CreateAsync(movie, cancellationToken);
        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        // ALWAYS return contracts, not domain models
        var response = movie.MapToResponse();
        // When new item is created in a REST API, Return a Created/201 response
        // This Should include the proper contract response and location URI
        return CreatedAtAction(nameof(Get), new { idOrSlug = response.Id }, response);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    [ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    [OutputCache(PolicyName = "Movies")]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug,
        [FromServices] LinkGenerator linkGenerator,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

        if (movie == null) return NotFound();

        var response = movie.MapToResponse();

        var movieObj = new { id = movie.Id };
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Get), values: new { idOrSlug = movie.Id })!,
            Rel = "self",
            Type = "GET"
        });

        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: new { idOrSlug = movie.Id })!,
            Rel = "self",
            Type = "PUT"
        });

        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: new { idOrSlug = movie.Id })!,
            Rel = "self",
            Type = "DELETE"
        });

        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Get), values: new { idOrSlug = movie.Id })!,
            Rel = "self",
            Type = "GET"
        });
        return Ok();
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    [OutputCache(PolicyName = "MovieCache")]
    [ResponseCache(Duration = 30, VaryByQueryKeys = ["title", "year", "sortBy", "page", "pageSize"],
        VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var options = request.MapToOptions().WithUser(userId);
        var movies = await _movieService.GetAllAsync(options, cancellationToken);
        var movieCount = await _movieService.GetCountAsync(options, cancellationToken);
        return Ok(movies.MapToResponse(
            request.Page.GetValueOrDefault(PagedRequest.DefaultPage),
            request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize),
            movieCount));
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var movie = request.MapToMovie();
        var updatedMovie = await _movieService.UpdateAsync(movie, userId, cancellationToken);

        if (updatedMovie is null) return NotFound();

        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        var response = movie.MapToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await _movieService.DeleteByIdAsync(id, cancellationToken);
        if (!isDeleted) return NotFound();
        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        return NoContent();
    }
}