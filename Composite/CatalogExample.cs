namespace DesignPatterns.Composite;

public interface ICatalogItem
{
    string Name { get; }
    decimal Price { get; }
    string Describe(int depth = 0);
}

public sealed class Product(string name, decimal price) : ICatalogItem
{
    public string Name { get; } = name;
    public decimal Price { get; } = price;

    public string Describe(int depth = 0) =>
        $"{Indent(depth)}- {Name}: {Price:N0}";

    private static string Indent(int depth) => new(' ', depth * 2);
}

public sealed class ProductBundle(string name) : ICatalogItem
{
    private readonly List<ICatalogItem> _items = [];

    public string Name { get; } = name;
    public decimal Price => _items.Sum(item => item.Price);

    public void Add(ICatalogItem item)
    {
        if (ReferenceEquals(item, this))
            throw new InvalidOperationException("یک بسته نمی‌تواند عضو خودش باشد.");

        _items.Add(item);
    }

    public string Describe(int depth = 0)
    {
        var header = $"{new string(' ', depth * 2)}+ {Name}: {Price:N0}";
        var children = _items.Select(item => item.Describe(depth + 1));
        return string.Join(Environment.NewLine, new[] { header }.Concat(children));
    }
}

public static class CompositeDemo
{
    public static void Run()
    {
       
        Console.WriteLine();

        var office = new ProductBundle("بسته آفیس");
        office.Add(new Product("لایسنس ورد", 1_200_000));
        office.Add(new Product("لایسنس اکسل", 1_200_000));

        var starter = new ProductBundle("بسته شروع کار");
        starter.Add(new Product("لپ‌تاپ", 35_000_000));
        starter.Add(new Product("ماوس", 450_000));
        starter.Add(office);

        Console.WriteLine(starter.Describe());
        Console.WriteLine();
        Console.WriteLine($"قیمت کل، با همان متد Price: {starter.Price:N0}");
    }
}
