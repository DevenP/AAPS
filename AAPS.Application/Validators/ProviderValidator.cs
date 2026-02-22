using FluentValidation;
using AAPS.Application.DTO;

public class ProviderValidator : AbstractValidator<ProviderDTO>
{
    public ProviderValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Please enter a valid work email.");

        RuleFor(x => x.Ssn)
            .Matches(@"^\d{9}$").WithMessage("SSN must be exactly 9 digits (no dashes).")
            .When(x => !string.IsNullOrEmpty(x.Ssn));

        // Logic Validation
        RuleFor(x => x.License1Expiration)
            .GreaterThan(DateTime.Today).WithMessage("License cannot be expired.")
            .When(x => x.License1Expiration.HasValue);
    }

    // This helper makes MudBlazor happy
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ProviderDTO>.CreateWithOptions((ProviderDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
