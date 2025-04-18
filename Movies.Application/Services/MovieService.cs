using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repository;

namespace Movies.Application.Services;

public class MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator) : IMovieService
{
    
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await movieRepository.CreateAsync(movie, cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await movieRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await movieRepository.GetBySlugAsync(slug, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await movieRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    { 
       await movieValidator.ValidateAndThrowAsync(movie, cancellationToken); 
       var movieExists = await movieRepository.ExistByIdAsync(movie.Id, cancellationToken);
       if (!movieExists)
       {
           return null;
       }
       await movieRepository.UpdateAsync(movie, cancellationToken);
       return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await movieRepository.DeleteByIdAsync(id, cancellationToken);
    }
}