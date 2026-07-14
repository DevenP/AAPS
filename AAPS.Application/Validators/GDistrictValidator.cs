using FluentValidation;
using AAPS.Application.DTO;

public class GDistrictValidator : AbstractValidator<GDistrictDTO>
{
    public GDistrictValidator()
    {
        RuleFor(x => x.DistrictCode)
            .NotEmpty().WithMessage("District code is required")
            .MaximumLength(2).WithMessage("District code cannot exceed 2 characters");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<GDistrictDTO>.CreateWithOptions(
            (GDistrictDTO)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid) return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
