using System.ComponentModel.DataAnnotations;

namespace AAPS.Application.Common.Attributes;

/// <summary>
/// Combines [Browsable] and [Display] functionality into a single attribute
/// to reduce boilerplate in DTOs.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class DisplayFieldAttribute : Attribute
{
    public DisplayFieldAttribute(string displayName, bool browsable = true)
    {
        DisplayName = displayName;
        Browsable = browsable;
    }

    public string DisplayName { get; }
    public bool Browsable { get; }
    public string? Description { get; set; }
    public int Order { get; set; } = -1;
    public string? GroupName { get; set; }
}
