namespace AAPS.Application.Common.Settings;

public class BillingSettings
{
    /// <summary>Root path where generated billing files are saved. Files are placed under {OutputPath}/{year}/{month}/.</summary>
    public string OutputPath { get; set; } = "";
}
