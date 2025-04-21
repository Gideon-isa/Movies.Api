namespace Movies.Contracts.Request;

public class GetAllMovieRequest
{
    public required string? Title { get; init; }
    public required int? Year { get; init; }
    
}