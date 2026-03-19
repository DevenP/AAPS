using FluentValidation;
using AAPS.Application.DTO;

public class BillingRateValidator : AbstractValidator<BillingRateDTO>
{
    public BillingRateValidator()
    {
        RuleFor(x => x.District)
            .NotEmpty().WithMessage("District is required")
            .MaximumLength(2).WithMessage("District cannot exceed 2 characters");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("Service Type is required")
            .MaximumLength(50).WithMessage("Service Type cannot exceed 50 characters");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .MaximumLength(25).WithMessage("Language cannot exceed 25 characters");

        RuleFor(x => x.Rate)
            .NotNull().WithMessage("Rate is required")
            .GreaterThanOrEqualTo(1).WithMessage("Rate must be at least $1.00")
            .LessThanOrEqualTo(9999.99m).WithMessage("Rate cannot exceed $9,999.99");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<BillingRateDTO>.CreateWithOptions(
            (BillingRateDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
