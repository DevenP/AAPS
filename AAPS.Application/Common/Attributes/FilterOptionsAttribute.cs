namespace AAPS.Application.Common.Attributes;

/// <summary>
/// Specifies a fixed set of allowed filter values for a property.
/// The FilterDialog will render a dropdown instead of a free-text input.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FilterOptionsAttribute : Attribute
{
    public FilterOptionsAttribute(params string[] options)
    {
        Options = options;
    }

    public string[] Options { get; }
}
