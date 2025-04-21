namespace Movies.Contracts.Request;

public class GetAllMovieRequest : PageRequest
{
    public required string? Title { get; init; }
    public required int? Year { get; init; }
    public string? SortBy { get; init; }
    
}