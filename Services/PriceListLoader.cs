using System.Globalization;
using Avalonia.Media;
using ClosedXML.Excel;
using Vereinskasse.Models;

namespace Vereinskasse.Services;

public static class PriceListLoader
{
    private static readonly IBrush[] HeaderPalette =
    {
        new SolidColorBrush(Color.Parse("#2F80D8")),
        new SolidColorBrush(Color.Parse("#3DA35D")),
        new SolidColorBrush(Color.Parse("#E8871E")),
        new SolidColorBrush(Color.Parse("#D9B10A")),
        new SolidColorBrush(Color.Parse("#8E44AD")),
        new SolidColorBrush(Color.Parse("#16A5A5")),
    };

    private static readonly IBrush[] TintPalette =
    {
        new SolidColorBrush(Color.Parse("#E3F1FC")),
        new SolidColorBrush(Color.Parse("#E4F6E7")),
        new SolidColorBrush(Color.Parse("#FDEEDD")),
        new SolidColorBrush(Color.Parse("#FBF4D8")),
        new SolidColorBrush(Color.Parse("#F1E6F7")),
        new SolidColorBrush(Color.Parse("#E1F6F6")),
    };

    public static List<CategoryGroup> Load(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();

        var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in sheet.Row(1).CellsUsed())
        {
            columnIndex[cell.GetString().Trim()] = cell.Address.ColumnNumber;
        }

        var colProdukt = columnIndex["Produkt"];
        var colPreis = columnIndex["Preis"];
        var colKategorie = columnIndex["Kategorie"];
        var colPosition = columnIndex["Position"];

        var categoryOrder = new List<string>();
        var productsByCategory = new Dictionary<string, List<Product>>();

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var r = 2; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            if (row.IsEmpty()) continue;

            var name = row.Cell(colProdukt).GetString().Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            var priceText = row.Cell(colPreis).GetString().Trim().Replace(',', '.');
            decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price);

            var category = row.Cell(colKategorie).GetString().Trim();
            int.TryParse(row.Cell(colPosition).GetString().Trim(), out var position);

            if (!productsByCategory.TryGetValue(category, out var list))
            {
                list = new List<Product>();
                productsByCategory[category] = list;
                categoryOrder.Add(category);
            }

            list.Add(new Product(name, price, category, position));
        }

        var result = new List<CategoryGroup>();
        for (var i = 0; i < categoryOrder.Count; i++)
        {
            var category = categoryOrder[i];
            var header = HeaderPalette[i % HeaderPalette.Length];
            var tint = TintPalette[i % TintPalette.Length];

            var orderedProducts = productsByCategory[category]
                .OrderBy(p => p.Position)
                .Select(p => new Product(p.Name, p.Price, p.Category, p.Position) { Background = tint })
                .ToList();

            result.Add(new CategoryGroup(category, header, orderedProducts));
        }

        return result;
    }
}
