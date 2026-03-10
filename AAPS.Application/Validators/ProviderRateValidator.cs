using FluentValidation;
using AAPS.Application.DTO;

public class ProviderRateValidator : AbstractValidator<ProviderRateDTO>
{
    public ProviderRateValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotNull().WithMessage("Provider is required");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("Service Type is required")
            .MaximumLength(50);

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("District is required")
            .MaximumLength(50);

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .MaximumLength(50);

        RuleFor(x => x.Rate)
            .NotNull().WithMessage("Rate is required")
            .GreaterThan(0).WithMessage("Rate must be greater than 0");

        RuleFor(x => x.EffectiveDate)
            .NotNull().WithMessage("Effective date is required")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Effective date cannot be in the future");
    }

    // This helper makes MudBlazor happy
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ProviderRateDTO>.CreateWithOptions((ProviderRateDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}

