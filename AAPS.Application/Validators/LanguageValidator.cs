using FluentValidation;
using AAPS.Application.DTO;

public class LanguageValidator : AbstractValidator<LanguageDTO>
{
    public LanguageValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(25).WithMessage("Name cannot exceed 25 characters");

        RuleFor(x => x.Code)
            .MaximumLength(2).WithMessage("Code cannot exceed 2 characters");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<LanguageDTO>.CreateWithOptions(
            (LanguageDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
