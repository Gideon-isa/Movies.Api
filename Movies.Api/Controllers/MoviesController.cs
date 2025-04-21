using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Request;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController(IMovieService movieService) : ControllerBase
{
    [Authorize(Policy = AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndPoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody]  CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await movieService.CreateAsync(movie, cancellationToken);
        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
    }

    [HttpGet(ApiEndPoints.Movies.Get)]
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
    public async Task<IActionResult> GetAll([FromQuery] GetAllMovieRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions().WithUser(userId);
        var movies = await movieService.GetAllAsync(options, cancellationToken);
        return Ok(movies.MapToMovieResponse());
    }

    [Authorize(Policy = AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(ApiEndPoints.Movies.Update)]
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
        var response = updatedMovie.MapToResponse();
        return Ok(response);
    }
    
    [Authorize(Policy = AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndPoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await movieService.DeleteByIdAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return Ok();
    }
} 