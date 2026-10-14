using System.Text;
using DesignPatterns.Adapter;
using DesignPatterns.Composite;
using DesignPatterns.Facade;
using DesignPatterns.Proxy;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length > 0)
{
    await RunAsync(args[0], args);
    return;
}

while (true)
{
    Console.WriteLine("""
        
          1  Composite
          2  Proxy
          3  Adapter   (MailKit → Gmail)
          4  Facade    (MailKit → Gmail)
          0  exit   
        """);
    Console.Write("> ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            CompositeDemo.Run();
            break;
        case "2":
            ProxyDemo.Run();
            break;
        case "3":
            await AdapterDemo.RunAsync([]);
            break;
        case "4":
            await FacadeDemo.RunAsync([]);
            break;
        case "0":
            return;
        default:
            Console.WriteLine("گزینه نامعتبر است.");
            break;
    }

    Console.WriteLine();
}

static async Task RunAsync(string name, string[] args)
{
    switch (name.ToLowerInvariant())
    {
        case "1":
        case "composite":
            CompositeDemo.Run();
            break;
        case "2":
        case "proxy":
            ProxyDemo.Run();
            break;
        case "3":
        case "adapter":
            await AdapterDemo.RunAsync(args);
            break;
        case "4":
        case "facade":
            await FacadeDemo.RunAsync(args);
            break;
        default:
           
            break;
    }
}
