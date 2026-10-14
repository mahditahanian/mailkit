namespace DesignPatterns.Proxy;

public interface IDocument
{
    string Title { get; }
    string Read();
}

public sealed class ConfidentialDocument : IDocument
{
    public ConfidentialDocument(string title, string content)
    {
        Title = title;
        Content = content;
        Console.WriteLine($"[منبع] سند «{title}» از پایگاه بارگذاری شد.");
    }

    public string Title { get; }
    private string Content { get; }

    public string Read() => Content;
}

public sealed class DocumentProxy : IDocument
{
    private readonly bool _allowed;
    private readonly Func<IDocument> _load;
    private IDocument? _real;

    public DocumentProxy(string title, bool allowed, Func<IDocument> load)
    {
        Title = title;
        _allowed = allowed;
        _load = load;
    }

    public string Title { get; }

    public string Read()
    {
        if (!_allowed)
            return $"دسترسی به «{Title}» مجاز نیست.";

        _real ??= _load();
        return _real.Read();
    }
}

public static class ProxyDemo
{
    public static void Run()
    {
      
        Console.WriteLine();

        var loads = 0;
        IDocument Load()
        {
            loads++;
            return new ConfidentialDocument("حقوق مهر", "مبلغ این ماه: ۱۲۴۰۰۰۰۰۰ ریال");
        }

        IDocument employeeView = new DocumentProxy("حقوق مهر", allowed: false, Load);
        Console.WriteLine($"کارمند: {employeeView.Read()}");
        Console.WriteLine($"تعداد بارگذاری بعد از رد شدن: {loads}");
        Console.WriteLine();

        IDocument managerView = new DocumentProxy("حقوق مهر", allowed: true, Load);
        Console.WriteLine($"مدیر، بار اول: {managerView.Read()}");
        Console.WriteLine($"مدیر، بار دوم: {managerView.Read()}");
        Console.WriteLine($"تعداد بارگذاری بعد از دو خواندن: {loads}");
    }
}
