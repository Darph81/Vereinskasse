using System.Collections.ObjectModel;
using Avalonia.Media;

namespace Vereinskasse.Models;

public class CategoryGroup
{
    public string Name { get; }
    public IBrush HeaderBackground { get; }
    public ObservableCollection<Product> Products { get; }

    public CategoryGroup(string name, IBrush headerBackground, IReadOnlyList<Product> products)
    {
        Name = name;
        HeaderBackground = headerBackground;
        Products = new ObservableCollection<Product>(products);
    }
}
