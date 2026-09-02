using FluentValidation;
using AAPS.Application.DTO;
using AAPS.Application.Common;

public class EvalValidator : AbstractValidator<EvalDTO>
{
    public EvalValidator()
    {
        RuleFor(x => x.StudentLastName)
            .NotEmpty().WithMessage("Student Last Name is required");

        RuleFor(x => x.StudentFirstName)
            .NotEmpty().WithMessage("Student First Name is required");

        // Phone is optional, but a full 10 digits if entered (the mask used to guarantee it).
        RuleFor(x => x.Phone)
            .Must(p => InputFormat.Digits(p).Length == 10)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone must be 10 digits.");

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
