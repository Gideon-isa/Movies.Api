using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(m => m.YearOfRelease)
            .LessThanOrEqualTo(DateTime.Now.Year);
        
    }
}