using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

public interface IConsentReportService
{
    /// <summary>Supply the physical path to the logo image (wwwroot/images/doe-logo.png).</summary>
    void SetLogoPath(string? path);

    /// <summary>
    /// Generates the NYC DOE Consent Letter PDF for a single evaluation and returns the raw bytes.
    /// </summary>
    byte[] GenerateConsentPdf(EvalDTO eval);
}
