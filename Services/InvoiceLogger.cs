using System.Globalization;
using ClosedXML.Excel;

namespace Vereinskasse.Services;

public static class InvoiceLogger
{
    public static string BuildFilePath(string baseDirectory)
    {
        var fileName = DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture) + "_Rechnung.xlsx";
        return Path.Combine(baseDirectory, "Rechnungen", fileName);
    }

    public static void EnsureCreated(string filePath, IReadOnlyList<string> categoryNames)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(filePath))
        {
            return;
        }

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Rechnungen");
        WriteHeader(sheet, categoryNames);
        workbook.SaveAs(filePath);
    }

    public static void AppendOrder(
        string filePath,
        decimal total,
        IReadOnlyList<string> categoryNames,
        IReadOnlyDictionary<string, int> quantitiesByCategory)
    {
        using var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();
        var sheet = workbook.Worksheets.FirstOrDefault();
        if (sheet == null)
        {
            sheet = workbook.Worksheets.Add("Rechnungen");
            WriteHeader(sheet, categoryNames);
        }

        var newRow = (sheet.LastRowUsed()?.RowNumber() ?? 0) + 1;
        if (newRow == 1)
        {
            WriteHeader(sheet, categoryNames);
            newRow = 2;
        }

        sheet.Cell(newRow, 1).Value = total;
        for (var i = 0; i < categoryNames.Count; i++)
        {
            var quantity = quantitiesByCategory.TryGetValue(categoryNames[i], out var q) ? q : 0;
            sheet.Cell(newRow, i + 2).Value = quantity;
        }

        workbook.SaveAs(filePath);
    }

    private static void WriteHeader(IXLWorksheet sheet, IReadOnlyList<string> categoryNames)
    {
        sheet.Cell(1, 1).Value = "Gesamt";
        for (var i = 0; i < categoryNames.Count; i++)
        {
            sheet.Cell(1, i + 2).Value = categoryNames[i];
        }

        sheet.Row(1).Style.Font.Bold = true;
    }
}
