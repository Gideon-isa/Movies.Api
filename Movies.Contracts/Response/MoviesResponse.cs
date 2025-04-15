namespace Movies.Contracts.Response;

public class MoviesResponse
{
    public required IEnumerable<MovieResponse> Items { get; init; } = [];
}