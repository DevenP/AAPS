using System.Text;

namespace AAPS.Application.Common.Helpers
{
    public static class CsvHelper
    {
        public static string ToCsv<T>(IEnumerable<T> items, string[] headers, Func<T, object?[]> rowMapper)
        {
            var csv = new StringBuilder();
            csv.AppendLine(string.Join(",", headers));

            foreach (var item in items)
            {
                var values = rowMapper(item).Select(v => {
                    var val = v?.ToString() ?? "";
                    // Escape quotes and wrap in quotes if it contains commas or newlines
                    if (val.Contains(",") || val.Contains("\"") || val.Contains("\n"))
                        val = $"\"{val.Replace("\"", "\"\"")}\"";
                    return val;
                });
                csv.AppendLine(string.Join(",", values));
            }
            return csv.ToString();
        }
    }

}
