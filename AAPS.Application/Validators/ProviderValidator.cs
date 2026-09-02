using FluentValidation;
using AAPS.Application.DTO;
using AAPS.Application.Common;

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

        // SSN is optional when adding a provider, but if one is entered it has to be a full
        // 9 digits (the input mask used to guarantee this; it's a plain field now). Uniqueness
        // is enforced server-side in ProviderService on save.
        RuleFor(x => x.Ssn)
            .Must(s => InputFormat.Digits(s).Length == 9)
            .When(x => !string.IsNullOrWhiteSpace(x.Ssn))
            .WithMessage("SSN must be 9 digits.");

        // Phone is optional, but a full 10 digits if entered (the mask used to guarantee it).
        RuleFor(x => x.Phone)
            .Must(p => InputFormat.Digits(p).Length == 10)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone must be 10 digits.");
    }

    // This helper makes MudBlazor happy
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ProviderDTO>.CreateWithOptions((ProviderDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
