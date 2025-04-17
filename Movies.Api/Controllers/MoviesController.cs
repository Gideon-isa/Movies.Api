using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Respository;
using Movies.Application.Services;
using Movies.Contracts.Request;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController(IMovieService movieService) : ControllerBase
{
    [HttpPost(ApiEndPoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody]  CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await movieService.CreateAsync(movie);
        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
    }

    [HttpGet(ApiEndPoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) 
            ? await movieService.GetByIdAsync(id) 
            : await movieService.GetBySlugAsync(idOrSlug);
        
        if (movie == null)
        {
            return NotFound();
        }
        return Ok(movie.MapToResponse());
    }

    [HttpGet(ApiEndPoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
      var movies = await movieService.GetAllAsync();
      return Ok(movies.MapToMovieResponse());
    }

    [HttpPut(ApiEndPoints.Movies.Update)]
    public async Task<IActionResult> Update([FromBody] UpdateMovieRequest request, [FromRoute] Guid id)
    {
        var movie = request.MapToMovie(id);
        var updatedMovie = await movieService.UpdateAsync(movie);
        if (updatedMovie is null)
        {
            return NotFound();
        }
        var response = updatedMovie.MapToResponse();
        return Ok(response);
    }
    
    [HttpDelete(ApiEndPoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await movieService.DeleteByIdAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return Ok();
    }
} 