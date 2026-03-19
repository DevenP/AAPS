using ClosedXML.Excel;

namespace AAPS.Web.Helpers;

public static class ExcelExportHelper
{
    public static byte[] ToExcel<T>(IEnumerable<T> items, string[] headers, Func<T, object?[]> rowMapper)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Data");

        // Header row
        for (int col = 0; col < headers.Length; col++)
        {
            var cell = ws.Cell(1, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        int row = 2;
        foreach (var item in items)
        {
            var values = rowMapper(item);
            for (int col = 0; col < values.Length; col++)
            {
                var cell = ws.Cell(row, col + 1);
                var val = values[col];

                switch (val)
                {
                    case DateTime dt:
                        cell.Value = dt;
                        cell.Style.DateFormat.Format = "MM/dd/yyyy";
                        break;
                    case bool b:
                        cell.Value = b;
                        break;
                    case int i:
                        cell.Value = i;
                        break;
                    case decimal d:
                        cell.Value = d;
                        break;
                    case double db:
                        cell.Value = db;
                        break;
                    case null:
                        break;
                    default:
                        cell.Value = val.ToString();
                        break;
                }
            }
            row++;
        }

        ws.Columns().AdjustToContents(1, 60); // cap column width at 60

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
