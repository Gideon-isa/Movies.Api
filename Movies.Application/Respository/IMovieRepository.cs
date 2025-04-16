using Movies.Application.Models;

namespace Movies.Application.Respository;

public interface IMovieRepository
{
    Task<bool> CreateAsync(Movie movie);
    Task<Movie?> GetByIdAsync (Guid id);
    Task<Movie?> GetBySlugAsync (string Slug);
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<bool> UpdateAsync(Movie movie);
    Task<bool> DeleteByIdAsync(Guid id);
    
    Task<bool> ExistByIdAsync(Guid id);
    
}