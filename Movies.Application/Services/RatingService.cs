using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Models;
using Movies.Application.Repository;

namespace Movies.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMovieRepository _movieRepository;

    public RatingService(IRatingRepository ratingRatingRepository, IMovieRepository movieRepository)
    {
       _ratingRepository = ratingRatingRepository;
       _movieRepository = movieRepository;
    }
    
    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userid, CancellationToken cancellationToken = default)
    {
        if (rating is <= 0 or > 5)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = "Rating",
                    ErrorMessage = "Rating must be between 0 and 5"
                }
            ]);
        }

        var movieExists = await _movieRepository.ExistByIdAsync(movieId, cancellationToken);
        if (!movieExists)
        {
            return false;
        }
        return await _ratingRepository.RateMovieAsync(movieId, rating, userid, cancellationToken);
    }

    public Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        return _ratingRepository.DeleteRatingAsync(movieId, userId, cancellationToken);
    }

    public Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _ratingRepository.GetRatingsForUserAsync(userId, cancellationToken);
    }
}   