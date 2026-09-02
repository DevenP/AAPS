namespace AAPS.Application.Common;

/// <summary>
/// Normalizes free-typed SSN / phone entry into the stored dashed format. The entry fields no
/// longer use an input mask (the mask fought the caret on Blazor Server, sending it to the start
/// of the field on every keystroke), so the value is tidied up here on save instead.
/// </summary>
public static class InputFormat
{
    // 9 digits -> "xxx-xx-xxxx". Anything else is kept as typed (trimmed), so partial or unusual
    // values aren't silently mangled.
    public static string? Ssn(string? value)
    {
        var digits = Digits(value);
        return digits.Length == 9
            ? $"{digits[..3]}-{digits.Substring(3, 2)}-{digits[5..]}"
            : value?.Trim();
    }

    // 10 digits -> "xxx-xxx-xxxx". Anything else is kept as typed (trimmed).
    public static string? Phone(string? value)
    {
        var digits = Digits(value);
        return digits.Length == 10
            ? $"{digits[..3]}-{digits.Substring(3, 3)}-{digits[6..]}"
            : value?.Trim();
    }

    // Just the digit characters of a value (drops dashes, spaces, parentheses).
    public static string Digits(string? value) =>
        value is null ? string.Empty : new string(value.Where(char.IsDigit).ToArray());
}
