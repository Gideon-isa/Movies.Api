using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repository;
using Movies.Application.Services;

namespace Movies.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieRepository _repository;
    
    public MovieValidator(IMovieRepository repository)
    {
        _repository = repository;
        
        RuleFor(m => m.Id)
            .NotEmpty();

        RuleFor(m => m.Genres)
            .NotEmpty();
 
        RuleFor(m => m.Title)
            .NotEmpty();
        
        RuleFor(m => m.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(m => m.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("This movie already exists in the system.");
    }

    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken cancellationToken = default)
    {
        var existingMovie = await _repository.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (existingMovie is not null)
        {
            return existingMovie!.Id == movie.Id;
        }

        return existingMovie is null;
    }
}