using System.Globalization;
using System.Windows.Input;
using Avalonia.Media;

namespace Vereinskasse.Models;

public class Product
{
    public string Name { get; }
    public decimal Price { get; }
    public string Category { get; }
    public int Position { get; }

    public IBrush Background { get; init; } = Brushes.White;

    public string PriceText => Price.ToString("0.00", CultureInfo.InvariantCulture) + " €";

    public ICommand? AddCommand { get; set; }

    public Product(string name, decimal price, string category, int position)
    {
        Name = name;
        Price = price;
        Category = category;
        Position = position;
    }
}
