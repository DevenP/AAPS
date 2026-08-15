namespace AAPS.Application.Common.Attributes;

/// <summary>
/// The lookup list a filter dropdown should be populated from at runtime.
/// </summary>
public enum FilterSource
{
    ServiceType,
    District,
    Language
}

/// <summary>
/// Marks a string property whose advanced-search filter should be a dropdown populated from a
/// live lookup list (Service Types, Districts, Languages) rather than a free-text box.
/// Use <see cref="FilterOptionsAttribute"/> instead when the choices are a fixed set.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FilterOptionsSourceAttribute : Attribute
{
    public FilterOptionsSourceAttribute(FilterSource source)
    {
        Source = source;
    }

    public FilterSource Source { get; }
}
