using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TVM_CalcUI.Models;

public partial class OrderLine : ObservableObject
{
    public string Name { get; }
    public decimal Price { get; }
    public string Category { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotalText))]
    private int _quantity;

    public string PriceText => Price.ToString("0.00", CultureInfo.InvariantCulture) + " €";
    public string LineTotalText => (Price * Quantity).ToString("0.00", CultureInfo.InvariantCulture) + " €";

    public ICommand? RemoveCommand { get; set; }

    public OrderLine(string name, decimal price, string category)
    {
        Name = name;
        Price = price;
        Category = category;
        Quantity = 1;
    }
}
