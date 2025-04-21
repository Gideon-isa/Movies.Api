using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repository;

namespace Movies.Application.Services;

public class MovieService(
    IMovieRepository movieRepository, 
    IRatingRepository ratingRepository, 
    IValidator<Movie> movieValidator, 
    IValidator<GetAllMoviesOptions> optionsValidator) : IMovieService
{
    
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await movieRepository.CreateAsync(movie, cancellationToken);
    }

    public Task<Movie?> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken cancellationToken = default)
    {
        return movieRepository.GetByIdAsync(id, userid, cancellationToken);
    }

    public Task<Movie?> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken cancellationToken = default)
    {
        return movieRepository.GetBySlugAsync(slug, userid, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
    {
        await optionsValidator.ValidateAndThrowAsync(options, cancellationToken);
        return await movieRepository.GetAllAsync(options, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userid = default, CancellationToken cancellationToken = default)
    { 
       await movieValidator.ValidateAndThrowAsync(movie, cancellationToken); 
       var movieExists = await movieRepository.ExistByIdAsync(movie.Id, cancellationToken);
       if (!movieExists)
       {
           return null;
       }
       await movieRepository.UpdateAsync(movie, cancellationToken);
    
       if (!userid.HasValue)
       {
           var rating = await ratingRepository.GetRatingAsync(movie.Id,cancellationToken);
           movie.Rating = rating;
           return movie;
       }
       var ratings = await ratingRepository.GetRatingAsync(movie.Id, userid.Value, cancellationToken);
       movie.Rating = ratings.Rating;
       movie.UserRating = ratings.UserRating;
       return movie;
    } 

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await movieRepository.DeleteByIdAsync(id, cancellationToken);
    }

    public Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default)
    {
        return movieRepository.GetCountAsync(title, yearOfRelease, cancellationToken);
    }
}