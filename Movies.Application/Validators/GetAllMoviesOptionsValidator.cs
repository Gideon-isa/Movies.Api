using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    private static readonly string[] AcceptableSortField =
    {
        "title", "yearofrelease"
    };
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(m => m.YearOfRelease)
            .LessThanOrEqualTo(DateTime.Now.Year);

        RuleFor(m => m.SortField)
            .Must(m => m is null || AcceptableSortField.Contains(m, StringComparer.OrdinalIgnoreCase))
            .WithMessage("You can only sort by 'title' or 'yearofrelease'");
    }
}