using FluentValidation;
using AAPS.Application.DTO;

public class ServiceTypeValidator : AbstractValidator<ServiceTypeDTO>
{
    public ServiceTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ServiceTypeDTO>.CreateWithOptions(
            (ServiceTypeDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
