using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vereinskasse.Models;
using Vereinskasse.Services;

namespace Vereinskasse.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<CategoryGroup> Categories { get; } = new();
    public ObservableCollection<OrderLine> OrderLines { get; } = new();

    [ObservableProperty]
    private string _totalText = "0.00 €";

    [ObservableProperty]
    private string _gegebenText = string.Empty;

    [ObservableProperty]
    private string _rueckgeldText = string.Empty;

    [ObservableProperty]
    private string _clockText = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);

    [ObservableProperty]
    private string? _loadError;

    private readonly DispatcherTimer _clockTimer;
    private readonly string _invoiceFilePath;

    public MainWindowViewModel()
    {
        LoadPriceList();

        _invoiceFilePath = InvoiceLogger.BuildFilePath(AppContext.BaseDirectory);
        try
        {
            InvoiceLogger.EnsureCreated(_invoiceFilePath, Categories.Select(c => c.Name).ToList());
        }
        catch (Exception ex)
        {
            LoadError = $"Rechnungsdatei konnte nicht angelegt werden: {ex.Message}";
        }

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => ClockText = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);
        _clockTimer.Start();
    }

    private void LoadPriceList()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "config", "Preisliste.xlsx");
            var groups = PriceListLoader.Load(path);

            foreach (var group in groups)
            {
                foreach (var product in group.Products)
                {
                    product.AddCommand = AddProductCommand;
                }

                Categories.Add(group);
            }
        }
        catch (Exception ex)
        {
            LoadError = $"Preisliste konnte nicht geladen werden: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddProduct(Product product)
    {
        var existing = OrderLines.FirstOrDefault(l => l.Name == product.Name && l.Price == product.Price);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            var line = new OrderLine(product.Name, product.Price, product.Category) { RemoveCommand = RemoveLineCommand };
            OrderLines.Add(line);
        }

        RecalculateTotal();
    }

    [RelayCommand]
    private void RemoveLine(OrderLine line)
    {
        line.Quantity--;
        if (line.Quantity <= 0)
        {
            OrderLines.Remove(line);
        }

        RecalculateTotal();
    }

    [RelayCommand]
    private void Digit(string digit)
    {
        if (digit == "," && GegebenText.Contains(','))
        {
            return;
        }

        GegebenText += digit;
    }

    [RelayCommand]
    private void Backspace()
    {
        if (GegebenText.Length > 0)
        {
            GegebenText = GegebenText[..^1];
        }
    }

    [RelayCommand]
    private void ClearGegeben()
    {
        GegebenText = string.Empty;
    }

    [RelayCommand]
    private void ClearOrder()
    {
        ResetOrder();
    }

    [RelayCommand]
    private void CompletePayment()
    {
        if (OrderLines.Count > 0)
        {
            LogOrder();
        }

        ResetOrder();
    }

    private void LogOrder()
    {
        try
        {
            var categoryNames = Categories.Select(c => c.Name).ToList();
            var quantitiesByCategory = OrderLines
                .GroupBy(l => l.Category)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

            InvoiceLogger.AppendOrder(_invoiceFilePath, CurrentTotal, categoryNames, quantitiesByCategory);
        }
        catch (Exception ex)
        {
            LoadError = $"Rechnung konnte nicht gespeichert werden: {ex.Message}";
        }
    }

    private void ResetOrder()
    {
        OrderLines.Clear();
        GegebenText = string.Empty;
        RecalculateTotal();
    }

    private decimal CurrentTotal => OrderLines.Sum(l => l.Price * l.Quantity);

    private void RecalculateTotal()
    {
        TotalText = CurrentTotal.ToString("0.00", CultureInfo.InvariantCulture) + " €";
        RecalculateRueckgeld();
    }

    partial void OnGegebenTextChanged(string value)
    {
        RecalculateRueckgeld();
    }

    private void RecalculateRueckgeld()
    {
        var normalized = GegebenText.Replace(',', '.');
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var gegeben))
        {
            var change = gegeben - CurrentTotal;
            RueckgeldText = change.ToString("0.00", CultureInfo.InvariantCulture) + " €";
        }
        else
        {
            RueckgeldText = string.Empty;
        }
    }
}
