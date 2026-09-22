using FluentValidation;
using AAPS.Application.DTO;

public class SettingValidator : AbstractValidator<SettingDTO>
{
    public SettingValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required")
            .MaximumLength(500).WithMessage("Value cannot exceed 500 characters");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<SettingDTO>.CreateWithOptions(
            (SettingDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
