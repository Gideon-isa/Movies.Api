using Movies.Application.Models;
using Movies.Contracts.Request;
using Movies.Contracts.Response;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
   public static Movie MapToMovie(this CreateMovieRequest request)
   {
      return new Movie
      {
         Id = Guid.NewGuid(),
         Title = request.Title,
         YearOfRelease = request.YearOfRelease,
         Genres = request.Genres.ToList()
      };
   }
   
   public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
   {
      return new Movie
      {
         Id = id,
         Title = request.Title,
         YearOfRelease = request.YearOfRelease,
         Genres = request.Genres.ToList()
      };
   }

   public static MovieResponse MapToResponse(this Movie movie)
   {
      return new MovieResponse()
      {
         Id = movie.Id,
         Title = movie.Title,
         Slug = movie.Slug,
         Rating = movie.Rating,
         UserRating = movie.UserRating,
         YearOfRelease = movie.YearOfRelease,
         Genres = movie.Genres.ToList()
      };
   }

   public static MoviesResponse MapToMovieResponse(this IEnumerable<Movie> movies)
   {
      return new MoviesResponse
      {
         Items = movies.Select<Movie, MovieResponse>(m => m.MapToResponse())
      };
   }

   public static IEnumerable<MovieRatingResponse> MapToResponse(this IEnumerable<MovieRating>  ratings )
   {
      return ratings.Select(r => new MovieRatingResponse
      {
         Rating = r.Rating,
         Slug = r.Slug,
         MovieId = r.MovieId,
      });
   }

   public static GetAllMoviesOptions MapToOptions(this GetAllMovieRequest request)
   {
      return new GetAllMoviesOptions
      {
         Title = request.Title,
         YearOfRelease = request.Year
      };
   }
   
   public static GetAllMoviesOptions WithUser(this GetAllMoviesOptions options, Guid? userId)
   {
      options.UserId = userId;
      return options;
   }
   
}