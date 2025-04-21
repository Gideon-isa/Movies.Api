using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repository;

public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
{
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
  
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                      insert into movies (id, slug, title, yearofrelease)
                                                      values (@id, @slug, @title, @yearofrelease)
                                                      """, movie, cancellationToken : cancellationToken));
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

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userid = default, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
            SELECT m.*, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS userrating 
            FROM movies m
            LEFT JOIN ratings r ON m.id = r.movieId
            LEFT JOIN ratings myr ON m.id = myr.movieId
            AND myr.userId = @userid
            WHERE id = @id
            GROUP BY id, userrating
            """, 
                new { id, userid }, cancellationToken: cancellationToken));
        
        if (movie is null)
        {
            return null;
        }
        
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("select name from genres where movieid = @id", 
                new { id }, cancellationToken: cancellationToken));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        return movie;
        
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userid = default, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                  SELECT m.*, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS userrating 
                  FROM movies m
                  LEFT JOIN ratings r ON m.id = r.movieId
                  LEFT JOIN ratings myr ON m.id = myr.movieId
                  AND myr.userId = @userid
                  WHERE id = @id
                  GROUP BY id, userrating
                  """,
                new { slug, userid }, cancellationToken: cancellationToken));
        
        if (movie is null)
        {
            return null;
        }
        
        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("select name from genres where movieid = @id", 
                new { id = movie.Id }, cancellationToken: cancellationToken));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }
        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var orderClause = string.Empty;
        
        if (options.SortField is not null)
        {
            orderClause = $"""
                           , m.{options.SortField} ORDER BY m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}
                           """;
        }
        
        var movies = await connection
            .QueryAsync(new CommandDefinition($"""
            SELECT m.*, string_agg(DISTINCT g.name, ',') AS genres, 
            ROUND(AVG(r.rating), 1) AS rating,
            myr.rating AS userrating
            FROM movies m 
            LEFT JOIN genres g on m.id = g.movieid 
            LEFT JOIN ratings r ON m.id = r.movieId
            LEFT JOIN ratings myr ON m.id = myr.movieId
            AND myr.userId = @userId
            WHERE (@title IS NULL OR m.title ILIKE ('%' || @title || '%' ))
            AND (@yearofrelease IS NULL OR m.yearofrelease = @yearofrelease)
            GROUP BY id, userrating {orderClause}
            LIMIT @pageSize OFFSET @pageOffset
            """, new
            {
                userId = options.UserId, 
                title = options.Title,  
                yearofrelease = options.YearOfRelease,
                pageSize = options.PageSize,
                pageOffset = (options.Page - 1) * options.PageSize,
            }, cancellationToken: cancellationToken));
        
        var movie =  movies.Select(movie => new Movie 
        {
            Id = movie.id,
            Title = movie.title,
            YearOfRelease = movie.yearofrelease,
            Rating = (float?)movie.rating,
            UserRating = (int?)movie.userrating,
            Genres = Enumerable.ToList(movie.genres.Split(','))
        });
        return movie;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            new CommandDefinition("""
                                  DELETE FROM genres WHERE movieid = @id
                                  """, new { id = movie.Id }, cancellationToken: cancellationToken));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(
                new CommandDefinition("""
                                      INSERT INTO genres (movieid, name) 
                                      VALUES (@MovieId, @name)
                                      """, new { MovieId = movie.Id, Name = genre }, cancellationToken: cancellationToken));
        }

        var result = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  UPDATE movies SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                                  WHERE id = @Id
                                  """, movie, cancellationToken: cancellationToken));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(
            new CommandDefinition("DELETE FROM genres WHERE movieid = @id", 
                new { id }, cancellationToken: cancellationToken));
        
        var result = await connection.ExecuteAsync(
            new CommandDefinition("DELETE FROM movies WHERE id = @id", 
                new { id }, cancellationToken: cancellationToken));
        
        transaction.Commit();
        return result > 0;
       
    }

    public async Task<bool> ExistByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition("SELECT COUNT(1) FROM movies WHERE id = @id", 
                new { id }, cancellationToken: cancellationToken));
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease,
        CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<int>(
            new CommandDefinition("""
                                  SELECT COUNT(id) FROM movies
                                  WHERE (@title IS NULL OR title LIKE ('%' || @title || '%' ))
                                  AND (@yearOfRelease IS NULL OR yearOfRelease = @yearOfRelease)
                                  """, new { title, yearOfRelease }, cancellationToken: cancellationToken));

    }
}