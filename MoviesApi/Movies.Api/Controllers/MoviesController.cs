using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController(IMovieService movieService) : ControllerBase
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
        var result = await _movieService.CreateAsync(movie, cancellationToken);
        // ALWAYS return contracts, not domain models
        var response = movie.MapToResponse();
        // When new item is created in a REST API, Return a Created/201 response
        // This Should include the proper contract response and location URI
        return CreatedAtAction(nameof(Get), new { idOrSlug = response.Id }, response);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

        if (movie == null) return NotFound();
        return Ok(movie.MapToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var movies = await _movieService.GetAllAsync(userId, cancellationToken);
        return Ok(movies.MapToResponse());
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
        var response = movie.MapToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await _movieService.DeleteByIdAsync(id, cancellationToken);
        if (!isDeleted) return NotFound();
        return NoContent();
    }
}