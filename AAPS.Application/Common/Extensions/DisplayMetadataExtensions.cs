using System.Reflection;
using AAPS.Application.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.Common.Extensions;

/// <summary>
/// Extensions for working with display metadata on DTO properties.
/// </summary>
public static class DisplayMetadataExtensions
{
    /// <summary>
    /// Gets the display name for a property using DisplayField or Display attribute.
    /// </summary>
    public static string? GetDisplayName(this PropertyInfo property)
    {
        var displayField = property.GetCustomAttribute<DisplayFieldAttribute>();
        if (displayField != null)
            return displayField.DisplayName;

        var display = property.GetCustomAttribute<DisplayAttribute>();
        return display?.Name;
    }

    /// <summary>
    /// Gets whether a property should be browsable/visible.
    /// </summary>
    public static bool IsBrowsable(this PropertyInfo property)
    {
        var displayField = property.GetCustomAttribute<DisplayFieldAttribute>();
        if (displayField != null)
            return displayField.Browsable;

        return true;
    }

    /// <summary>
    /// Gets whether a property is read-only.
    /// </summary>
    public static bool IsReadOnly(this PropertyInfo property)
    {
        return property.GetCustomAttribute<DisplayFieldAttribute>()?.IsReadOnly ?? false;
    }

    /// <summary>
    /// Gets all properties with their display metadata from a type.
    /// </summary>
    public static Dictionary<string, string> GetDisplayMetadata<T>() where T : class
    {
        var metadata = new Dictionary<string, string>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            var displayName = prop.GetDisplayName();
            if (!string.IsNullOrEmpty(displayName))
            {
                metadata[prop.Name] = displayName;
            }
        }

        return metadata;
    }

    /// <summary>
    /// Gets all browsable properties from a type.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetBrowsableProperties<T>() where T : class
    {
        return typeof(T).GetProperties().Where(p => p.IsBrowsable());
    }
}
