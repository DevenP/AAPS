namespace AAPS.Application.Common.Settings;

public class ImportSettings
{
    public string MandatesArchivePath { get; set; } = "";
    public string SesisArchivePath { get; set; } = "";
    public string VendorPortalArchivePath { get; set; } = "";
    public string PaymentsArchivePath { get; set; } = "";

    /// <summary>Maximum allowed file upload size in bytes. Default: 50MB.</summary>
    public long MaxFileSizeBytes { get; set; } = 50L * 1024 * 1024;

    /// <summary>Number of rows per batch when bulk inserting Sesis/VendorPortal records. Default: 500.</summary>
    public int BatchSize { get; set; } = 500;
}
