using FluentValidation;
using AAPS.Application.DTO;

public class OperationEditValidator : AbstractValidator<OperationEditDTO>
{
    public OperationEditValidator()
    {
        // EntryId (Approval ID) must be positive if provided
        RuleFor(x => x.EntryId)
            .GreaterThan(0).WithMessage("Approval ID must be a positive number.")
            .When(x => x.EntryId.HasValue);

        // Group size must be a positive integer if provided
        RuleFor(x => x.ActualSize)
            .Must(BeAPositiveInteger).WithMessage("Group size must be a positive whole number.")
            .When(x => !string.IsNullOrWhiteSpace(x.ActualSize));

        // Mandate end must not be before mandate start when both are provided
        RuleFor(x => x.MandateEnd)
            .GreaterThanOrEqualTo(x => x.MandateStart!.Value)
            .WithMessage("Approval End Date cannot be before Approval Start Date.")
            .When(x => x.MandateStart.HasValue && x.MandateEnd.HasValue);
    }

    private static bool BeAPositiveInteger(string? value) =>
        int.TryParse(value, out int n) && n > 0;

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<OperationEditDTO>.CreateWithOptions(
            (OperationEditDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
