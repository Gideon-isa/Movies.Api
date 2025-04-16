using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Respository;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();
  
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                      insert into movies (id, slug, title, yearofrelease)
                                                      values (@id, @slug, @title, @yearofrelease)
                                                      """, movie));
        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                                                                    insert into genres (movieId, name)
                                                                    values (@movieId, @name)
                                                                    """, new { MovieId = movie.Id, Name = genre }));

            }
        }
        throw new NotImplementedException();
    }

    public Task<Movie?> GetByIdAsync(Guid id)
    {
        // var movie = _movies.FirstOrDefault(m => m.Id == id);
        // return Task.FromResult(movie);
        throw new NotImplementedException();
    }
 

    public Task<IEnumerable<Movie>> GetAllAsync()
    {
         // return Task.FromResult(_movies.AsEnumerable());
         throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Movie movie)
    {
        // var movieIndex = _movies.FindIndex(m => m.Id == movie.Id);
        // if (movieIndex == -1)
        // {
        //     return Task.FromResult(false);
        // }
        // _movies[movieIndex] = movie;
        // return Task.FromResult(true);
        throw new NotImplementedException();
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
        // var removedCount =_movies.RemoveAll(m => m.Id == id);
        // var movieRemoved = removedCount > 0;
        // return Task.FromResult(movieRemoved); 
        throw new NotImplementedException();
    }

    public Task<bool> ExistByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Movie?>  GetBySlugAsync(string idOrSlug)
    {
        // var movie = _movies.SingleOrDefault(m => m.Slug.Contains(idOrSlug));
        // return Task.FromResult(movie);
        throw new NotImplementedException();
    }
}