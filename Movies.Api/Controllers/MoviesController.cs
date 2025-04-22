using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Request;
using Movies.Contracts.Response;

namespace Movies.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
public class MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore) : ControllerBase
{
    [Authorize(Policy = AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndPoints.Movies.Create)]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody]  CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await movieService.CreateAsync(movie, cancellationToken);
        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
    }

    [HttpGet(ApiEndPoints.Movies.Get)]
    // [ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    [OutputCache(PolicyName = "MoviesCache")]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId(); 
        var movie = Guid.TryParse(idOrSlug, out var id) 
            ? await movieService.GetByIdAsync(id, userId, cancellationToken) 
            : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
        
        if (movie == null)
        {
            return NotFound();
        }
        return Ok(movie.MapToResponse());
    }

    [HttpGet(ApiEndPoints.Movies.GetAll)]
    [OutputCache(PolicyName = "MoviesCache")]
    // [ResponseCache(Duration = 30, VaryByQueryKeys = ["title", "year", "sortBy", "page", "pageSize"], VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllMovieRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions().WithUser(userId);
        var movies = await movieService.GetAllAsync(options, cancellationToken);
        var movieCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);
        return Ok(movies.MapToMovieResponse(request.Page, request.PageSize, movieCount));
    }

    [Authorize(Policy = AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(ApiEndPoints.Movies.Update)]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateMovieRequest request, 
        [FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();
        var movie = request.MapToMovie(id);
        var updatedMovie = await movieService.UpdateAsync(movie, userId, cancellationToken);
        if (updatedMovie is null)
        {
            return NotFound(); 
        }
        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        var response = updatedMovie.MapToResponse();
        return Ok(response);
    }
    
    [Authorize(Policy = AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndPoints.Movies.Delete)]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await movieService.DeleteByIdAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        return Ok();
    }
} 