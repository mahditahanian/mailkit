using DesignPatterns.Gmail;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DesignPatterns.Adapter;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

public sealed class OrderNotifier(IEmailSender email)
{
    public Task ConfirmAsync(string customerEmail, string orderId, CancellationToken cancellationToken = default) =>
        email.SendAsync(
            customerEmail,
            $"سفارش {orderId}",
            $"سفارش {orderId} ثبت شد.",
            cancellationToken);
}

public sealed class GmailMailKitAdapter(GmailMailbox mailbox) : IEmailSender
{
    public static async Task LinkAsync(CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient { Timeout = 20_000 };
        try
        {
            await client.ConnectAsync(
                GmailMailbox.SmtpHost,
                GmailMailbox.SmtpPort,
                SecureSocketOptions.StartTls,
                cancellationToken);
            Console.WriteLine(
                $"به جیمیل وصل شد: {GmailMailbox.SmtpHost}:{GmailMailbox.SmtpPort}");
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, CancellationToken.None);
        }
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(mailbox.Address));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient { Timeout = 20_000 };
        try
        {
            await client.ConnectAsync(
                GmailMailbox.SmtpHost,
                GmailMailbox.SmtpPort,
                SecureSocketOptions.StartTls,
                cancellationToken);
            await client.AuthenticateAsync(mailbox.Address, mailbox.AppPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, CancellationToken.None);
        }
    }
}

public static class AdapterDemo
{
    public static async Task RunAsync(string[] args)
    {
      
        Console.WriteLine();

        await GmailMailKitAdapter.LinkAsync();

        if (!GmailMailbox.TryFromEnvironment(out var mailbox, out var hint) || mailbox is null)
        {
            Console.WriteLine(hint);
            return;
        }

        var to = Recipient(args) ?? mailbox.Address;
        if (!ConfirmSend(args, $"یک نامه از {mailbox.Address} به {to}"))
            return;

        IEmailSender sender = new GmailMailKitAdapter(mailbox);
        var orders = new OrderNotifier(sender);
        await orders.ConfirmAsync(to, "A-1042");
        Console.WriteLine("نامه از مسیر آداپتر فرستاده شد. OrderNotifier هیچ ارجاعی به MailKit ندارد.");
    }

    private static string? Recipient(string[] args) =>
        args.Length >= 2 && args[1].Contains('@') ? args[1].Trim() : null;

    private static bool ConfirmSend(string[] args, string action)
    {
        if (args.Length > 0)
            return true;

        Console.Write($"{action}. ارسال شود؟ (y/n): ");
        var answer = Console.ReadLine();
        return string.Equals(answer?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
    }
}
