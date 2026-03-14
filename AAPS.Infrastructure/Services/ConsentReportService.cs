using AAPS.Application.Abstractions.Services;
using AAPS.Application.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AAPS.Infrastructure.Services;

/// <summary>
/// Generates the NYC DOE "Student, Parent/Guardian and Independent Evaluator Information"
/// consent letter as a PDF, matching the original Crystal Report layout exactly.
/// LOGO: Place your logo image at wwwroot/images/nyc-doe-logo.png in AAPS.Web.
/// </summary>
public class ConsentReportService : IConsentReportService
{
    private string? _logoPath;

    private const string HeaderBg = "#4d4d4d"; // dark grey used for all header bars

    public ConsentReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void SetLogoPath(string? path) => _logoPath = path;

    public byte[] GenerateConsentPdf(EvalDTO eval)
    {
        return Document.Create(container =>
        {
            // Page 1 — data
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginHorizontal(32);
                page.MarginVertical(28);
                page.DefaultTextStyle(x => x.FontSize(7.5f).FontFamily("Arial"));
                page.Content().Element(c => ComposePage1(c, eval));
            });

            // Page 2 — static signatures
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginHorizontal(32);
                page.MarginVertical(28);
                page.DefaultTextStyle(x => x.FontSize(7.5f).FontFamily("Arial"));
                page.Content().Element(c => ComposePage2(c));
            });
        }).GeneratePdf();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PAGE 1
    // ─────────────────────────────────────────────────────────────────────────
    private void ComposePage1(IContainer container, EvalDTO eval)
    {
        container.Column(col =>
        {
            col.Spacing(0);

            // Logo
            col.Item().Element(c => RenderLogo(c));

            // Title bar
            col.Item()
               .Background(HeaderBg)
               .PaddingVertical(5).PaddingHorizontal(4)
               .AlignCenter()
               .Text("STUDENT, PARENT/GUARDIAN AND INDEPENDENT EVALUATOR INFORMATION")
               .Bold().FontColor(Colors.White).FontSize(7.5f);

            // Intro box — plain black thin border
            col.Item().Border(0.75f).BorderColor(Colors.Black).Padding(6).Column(inner =>
            {
                inner.Spacing(5);
                inner.Item().Text(text =>
                {
                    text.Span("Please complete and sign this form, attach a copy of the ");
                    text.Span("Evaluator's New York State Education Department (\"NYSED\") certificate/license and current registration")
                        .Bold();
                    text.Span(", and submit it to the ");
                    text.Span("\"Contact Person\" (Section I)").Bold();
                    text.Span(" for the New York City Department of Education (\"DOE\") for approval. Providers of bilingual assessments must also attach one of the following:");
                });
                inner.Item().PaddingLeft(20).Text("a.  the passing results of the NYSED Bilingual Education Assessment or other valid language proficiency assessment; or");
                inner.Item().PaddingLeft(20).Text("b.  an appropriate NYSED Bilingual Education Extension credential.");
            });

            col.Item().PaddingBottom(10);

            // Section I
            col.Item().Element(c => RenderSectionI(c, eval));

            col.Item().PaddingBottom(10);

            // Section II
            col.Item().Element(c => RenderSectionII(c, eval));

            col.Item().PaddingBottom(10);

            // Section III
            col.Item().Element(c => RenderSectionIII(c));

            col.Item().PaddingTop(20);

            // Rate line — text left, short line far right
            col.Item().Row(row =>
            {
                row.RelativeItem().AlignBottom()
                   .Text("Rate (cannot exceed the maximum allowed by the DOE)")
                   .FontSize(7.5f);
                row.ConstantItem(14).AlignBottom()
                   .LineHorizontal(0.75f).LineColor(Colors.Black);
            });

            col.Item().PaddingBottom(6);

            // Full-width line
            col.Item().LineHorizontal(0.75f).LineColor(Colors.Black);

            col.Item().PaddingBottom(3);

            // Footer note — bold italic indented
            col.Item()
               .PaddingLeft(16)
               .Text("If the agency tax identification number is included, the payment will be made to the agency.")
               .Bold().Italic().FontSize(7.5f);
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Logo
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderLogo(IContainer container)
    {
        container.Row(row =>
        {
            row.ConstantItem(110).Element(c =>
            {
                if (!string.IsNullOrEmpty(_logoPath) && File.Exists(_logoPath))
                {
                    var imageData = File.ReadAllBytes(_logoPath);
                    c.Height(78).Image(imageData).FitArea();
                }
                else
                {
                    c.Border(1).BorderColor(Colors.Grey.Lighten2)
                     .Background(Colors.Grey.Lighten4)
                     .Height(78).AlignCenter().AlignMiddle()
                     .Text("NYC DOE Logo").FontSize(7).FontColor(Colors.Grey.Medium);
                }
            });
            row.RelativeItem();
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Section I
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderSectionI(IContainer container, EvalDTO eval)
    {
        var studentName = $"{eval.StudentFirstName} {eval.StudentLastName}".Trim();

        container.ShowEntire().Border(0.5f).BorderColor(Colors.Grey.Medium).Column(col =>
        {
            SectionHeader(col, "SECTION I.  TO BE COMPLETED BY NEW YORK CITY DEPARTMENT OF EDUCATION STAFF");

            col.Item().Padding(5).Column(inner =>
            {
                inner.Spacing(5);

                inner.Item().Row(row =>
                {
                    row.RelativeItem(10).Element(c => Field(c, "Name of Student:", studentName));
                    row.RelativeItem(7).Element(c => Field(c, "Student's ID:", eval.StudentId ?? ""));
                });
                inner.Item().Row(row =>
                {
                    row.RelativeItem(10).Element(c => Field(c, "Assessment Requested:", eval.ServiceType ?? ""));
                    row.RelativeItem(7).Element(c => Field(c, "Language:", eval.Language ?? ""));
                });
                inner.Item().Row(row =>
                {
                    row.RelativeItem(10).Element(c => Field(c, "School:", ""));
                    row.RelativeItem(7).Element(c => Field(c, "Borough:", ""));
                });
                inner.Item().Element(c => Field(c, "District:", eval.District ?? ""));
                inner.Item().Row(row =>
                {
                    row.RelativeItem(10).Element(c => Field(c, "Contact Person:", ""));
                    row.RelativeItem(7).Element(c => Field(c, "Telephone #:", ""));
                });
                inner.Item().Element(c => Field(c, "Contact Person Address:", ""));
            });
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Section II
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderSectionII(IContainer container, EvalDTO eval)
    {
        var parentName = $"{eval.ParentFirstName} {eval.ParentLastName}".Trim();

        container.ShowEntire().Border(0.5f).BorderColor(Colors.Grey.Medium).Column(col =>
        {
            SectionHeader(col, "SECTION II.  TO BE COMPLETED BY PARENT/GUARDIAN");

            col.Item().Padding(5).Column(inner =>
            {
                inner.Spacing(5);

                inner.Item().Element(c => Field(c, "Name of Parent/Guardian:", parentName));
                inner.Item().Element(c => Field(c, "Address:", ""));
                inner.Item().Row(row =>
                {
                    row.RelativeItem(10).Element(c => Field(c, "Work Telephone No.:", ""));
                    row.RelativeItem(7).Element(c => Field(c, "Home Telephone No.:", eval.Phone ?? ""));
                });
                inner.Item().Row(row =>
                {
                    row.RelativeItem(10).Element(c => Field(c, "Evaluator Name:", ""));
                    row.RelativeItem(7).Element(c => Field(c, "Agency Name:", ""));
                });
                inner.Item().Element(c => Field(c, "Services to be provided at:", ""));
            });
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Section III
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderSectionIII(IContainer container)
    {
        container.ShowEntire().Border(0.5f).BorderColor(Colors.Grey.Medium).Column(col =>
        {
            SectionHeader(col, "SECTION III.  TO BE COMPLETED BY INDIVIDUAL EVALUATOR AND AGENCY (IF APPLICABLE):");

            col.Item().Padding(5).Column(inner =>
            {
                inner.Spacing(5);

                inner.Item().Element(c => Field(c, "Individual Evaluator name:", ""));

                inner.Item().Row(row =>
                {
                    row.RelativeItem(10).Column(c2 =>
                    {
                        c2.Spacing(5);
                        c2.Item().Element(c => Field(c, "Individual Evaluator's work address:", ""));
                        c2.Item().Element(c => Field(c, "Telephone No.:", ""));
                    });
                    row.RelativeItem(7).Column(c2 =>
                    {
                        c2.Spacing(5);
                        c2.Item().Element(c => Field(c, "Social Security Number:", ""));
                        c2.Item().Element(c => Field(c, "NYS License/Certificate No.:", ""));
                    });
                });

                // Original has the typo "Ttitle" — preserved intentionally
                inner.Item().Element(c => Field(c, "Ttitle/Discipline:", ""));

                // Dashed line separator
                inner.Item().Text("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                     .FontSize(6.5f).FontColor(Colors.Grey.Darken1);

                inner.Item().Row(row =>
                {
                    row.RelativeItem(10).Element(c => Field(c, "Agency name:", ""));
                    row.RelativeItem(7).Element(c => Field(c, "Agency tax Id:", ""));
                });

                inner.Item().Element(c => Field(c, "Agency address:", ""));
            });
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PAGE 2 — static signature page
    // ─────────────────────────────────────────────────────────────────────────
    private static void ComposePage2(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(0);

            // Top spacing
            col.Item().PaddingTop(55);

            // Agency signature block
            col.Item().Row(row =>
            {
                row.RelativeItem(5).Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(2).Text("Signature of Authorized Agency Representative").FontSize(7.5f);
                    c.Item().AlignCenter().Text("(if applicable)").FontSize(7.5f);
                });
                row.ConstantItem(24);
                row.RelativeItem(4).Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(2).Text("Print name").FontSize(7.5f);
                });
                row.ConstantItem(24);
                row.RelativeItem(2).Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(2).Text("Date").FontSize(7.5f);
                });
            });

            // Large gap
            col.Item().PaddingTop(70);

            // Parent consent paragraph
            col.Item().Text(
                "I, the Student's parent/guardian, have read the above terms of provision of services. " +
                "I have chosen the Individual Evaluator/Agency identified in Section II and grant permission " +
                "to the DOE to release the Student's records to the Individual Evaluator/Agency to the extent " +
                "necessary to conduct the evaluation."
            ).FontSize(7.5f);

            // Large gap
            col.Item().PaddingTop(70);

            // Parent/Guardian signature block — single line spanning all three
            col.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(2).Row(row =>
            {
                row.RelativeItem(5).Text("Signature of Parent/Guardian").FontSize(7.5f);
                row.RelativeItem(4).Text("Print name").FontSize(7.5f);
                row.RelativeItem(2).Text("Date").FontSize(7.5f);
            });
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────
    private static void SectionHeader(ColumnDescriptor col, string text)
    {
        col.Item()
           .Background(HeaderBg)
           .PaddingVertical(5).PaddingHorizontal(4)
           .Text(text)
           .Bold().FontColor(Colors.White).FontSize(7.5f);
    }

    private static void Field(IContainer container, string label, string value)
    {
        container.Text(text =>
        {
            text.Span(label).Bold();
            if (!string.IsNullOrEmpty(value))
            {
                text.Span("  ");
                text.Span(value);
            }
        });
    }
}