using FluentValidation;
using AAPS.Application.DTO;

public class EvalValidator : AbstractValidator<EvalDTO>
{
    public EvalValidator()
    {
        RuleFor(x => x.StudentLastName)
            .NotEmpty().WithMessage("Student Last Name is required");

        RuleFor(x => x.StudentFirstName)
            .NotEmpty().WithMessage("Student First Name is required");

        RuleFor(x => x.BillingAmount)
            .GreaterThanOrEqualTo(0).When(x => x.BillingAmount.HasValue)
            .WithMessage("Billing Amount must be a positive value");

        RuleFor(x => x.ProviderPaidAmount)
            .GreaterThanOrEqualTo(0).When(x => x.ProviderPaidAmount.HasValue)
            .WithMessage("Provider Paid Amount must be a positive value");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<EvalDTO>.CreateWithOptions((EvalDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
