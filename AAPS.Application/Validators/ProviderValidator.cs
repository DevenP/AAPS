using FluentValidation;
using AAPS.Application.DTO;

public class ProviderValidator : AbstractValidator<ProviderDTO>
{
    public ProviderValidator()
    {
        RuleFor(x => x.FirstName)
        .NotEmpty().WithMessage("First Name is required")
        .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required")
            .MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Please enter a valid work email.");

        // SSN is optional when adding a provider, but must be unique when supplied
        // (uniqueness is enforced server-side in ProviderService on create/update).
    }

    // This helper makes MudBlazor happy
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ProviderDTO>.CreateWithOptions((ProviderDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
