namespace AAPS.Application.Common;

/// <summary>
/// Helpers for paraprofessional (para) services, which work differently from the regular
/// related services: their approval duration is a percent of the school day rather than a fixed
/// session length, and they're billed by the hour. This is the one place that knows how to spot a
/// para, turn its percent into a daily minute cap, and map it to its billing code.
/// </summary>
public static class Para
{
    // A full (100%) school day for a para. Adjustable per approval afterward, but this is the
    // general mandate the DOE bills against.
    public const int FullDayMinutes = 420;

    // Settings key for the configurable 100% school-day length (falls back to FullDayMinutes).
    public const string FullDayMinutesSettingKey = "Para.FullDayMinutes";

    // A service type is a para if it starts with "Para" - covers the approval wording
    // ("Para - Health", "Para - Behavior Support") and the encounter wording ("Paraprofessional").
    public static bool IsPara(string? serviceType) =>
        serviceType?.TrimStart().StartsWith("Para", StringComparison.OrdinalIgnoreCase) ?? false;

    // Pulls the percent out of an approval duration like "100 Percent" or "80 Percent" (and the
    // "100%" form seen in the encounter file). Null when there's no percent to read.
    public static int? ParsePercent(string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration)) return null;
        var value = duration.Trim();
        if (value.IndexOf("percent", StringComparison.OrdinalIgnoreCase) < 0 && !value.Contains('%'))
            return null;

        var digits = new string(value.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var percent) ? percent : null;
    }

    // The minutes a para may bill per day for a given percent duration. The 100% basis defaults to
    // FullDayMinutes (420) but can be overridden with the value configured in Settings
    // (e.g. 100% -> 420, 80% -> 336). Null when the duration isn't a percent.
    public static int? DailyCapMinutes(string? duration, int fullDayMinutes = FullDayMinutes)
    {
        var percent = ParsePercent(duration);
        return percent is null
            ? null
            : (int)Math.Round(fullDayMinutes * percent.Value / 100m, MidpointRounding.AwayFromZero);
    }

    // The DOE billing code for a para type. Seeded with the two the client uses so far
    // (Health = HP, Behavior Support / Crisis = CP); extend as more types come in.
    public static string? CodeFor(string? serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType)) return null;
        var s = serviceType.ToLowerInvariant();
        if (s.Contains("health")) return "HP";
        if (s.Contains("behavior") || s.Contains("crisis")) return "CP";
        return null;
    }
}
