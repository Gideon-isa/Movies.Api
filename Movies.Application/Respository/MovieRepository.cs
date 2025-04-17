using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Respository;

public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
{
    public async Task<bool> CreateAsync(Movie movie)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
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
        transaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("select * from movies where id = @id", new { id }));
        
        if (movie is null)
        {
            return null;
        }
        
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("select name from genres where movieid = @id", new { id }));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        return movie;
        
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("select * from movies where slug = @slug", new { slug }));
        
        if (movie is null)
        {
            return null;
        }
        
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("select name from genres where movieid = @id", new { id = movie.Id }));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        var movies = await connection
            .QueryAsync(new CommandDefinition("""
            select m.*, string_agg(g.name, ',') as genres 
            from movies m left join genres g on m.id = g.movieid group by id
            """));

        return movies.Select(movie => new Movie
        {
            Id = movie.id,
            Title = movie.title,
            YearOfRelease = movie.yearofrelease,
            Genres = Enumerable.ToList(movie.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            new CommandDefinition("""
                                  DELETE FROM genres WHERE movieid = @id
                                  """, new { id = movie.Id }));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(
                new CommandDefinition("""
                                      INSERT INTO genres (movieid, name) 
                                      VALUES (@MovieId, @name)
                                      """, new { MovieId = movie.Id, Name = genre }));
        }

        var result = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  UPDATE movies SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                                  WHERE id = @Id
                                  """, movie));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(
            new CommandDefinition("DELETE FROM genres WHERE movieid = @id", new { id }));
        
        var result = await connection.ExecuteAsync(
            new CommandDefinition("DELETE FROM movies WHERE id = @id", new { id }));
        
        // var genres = await connection.QueryAsync<string>(
        //     new CommandDefinition("select name from genres where movieid = @id", new { id = movie.Id }));
        //
        // foreach (var genre in genres)
        // {
        //     movie.Genres.Add(genre);
        // }
        transaction.Commit();
        return result > 0;
       
    }

    public async Task<bool> ExistByIdAsync(Guid id)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition("""
                                  SELECT COUNT(1) FROM movies WHERE id = @id
                                  """, new { id }));
    }
}