using DesignPatterns.Gmail;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DesignPatterns.Facade;

public sealed record EmailDraft(string To, string Subject, string Body);

public sealed class GmailFacade(GmailMailbox mailbox)
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

    public Task SendAsync(EmailDraft draft, CancellationToken cancellationToken = default) =>
        SendManyAsync([draft], cancellationToken);

    public async Task SendManyAsync(IReadOnlyList<EmailDraft> drafts, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient { Timeout = 20_000 };
        try
        {
            await client.ConnectAsync(
                GmailMailbox.SmtpHost,
                GmailMailbox.SmtpPort,
                SecureSocketOptions.StartTls,
                cancellationToken);
            await client.AuthenticateAsync(mailbox.Address, mailbox.AppPassword, cancellationToken);

            foreach (var draft in drafts)
                await client.SendAsync(CreateMessage(draft), cancellationToken);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, CancellationToken.None);
        }
    }

    private MimeMessage CreateMessage(EmailDraft draft)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(mailbox.Address));
        message.To.Add(MailboxAddress.Parse(draft.To));
        message.Subject = draft.Subject;
        message.Body = new TextPart("plain") { Text = draft.Body };
        return message;
    }
}

public sealed class AccountMail(GmailFacade gmail)
{
    public Task SendPasswordResetAsync(string to, string token, CancellationToken cancellationToken = default) =>
        gmail.SendManyAsync(
        [
            new EmailDraft(to, "بازیابی رمز", $"کد بازیابی: {token}"),
            new EmailDraft(to, "هشدار امنیتی", "یک درخواست بازیابی رمز برای این حساب ثبت شد.")
        ],
        cancellationToken);
}

public static class FacadeDemo
{
    public static async Task RunAsync(string[] args)
    {
      
        Console.WriteLine();

        await GmailFacade.LinkAsync();

        if (!GmailMailbox.TryFromEnvironment(out var mailbox, out var hint) || mailbox is null)
        {
            Console.WriteLine(hint);
            return;
        }

        var to = args.Length >= 2 && args[1].Contains('@') ? args[1].Trim() : mailbox.Address;
        if (args.Length == 0)
        {
            Console.Write($"دو نامه از {mailbox.Address} به {to} روی یک اتصال. ارسال شود؟ (y/n): ");
            var answer = Console.ReadLine();
            if (!string.Equals(answer?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("نامه‌ای فرستاده نشد.");
                return;
            }
        }

        var account = new AccountMail(new GmailFacade(mailbox));
        await account.SendPasswordResetAsync(to, "482913");
        Console.WriteLine("هر دو نامه از پشت فساد فرستاده شدند. AccountMail قدم‌های MailKit را نمی‌بیند.");
    }
}
