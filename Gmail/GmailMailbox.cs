namespace DesignPatterns.Gmail;

public sealed record GmailMailbox(string Address, string AppPassword)
{
    public const string SmtpHost = "smtp.gmail.com";
    public const int SmtpPort = 587;

    public static bool TryFromEnvironment(out GmailMailbox? mailbox, out string hint)
    {
        var address = Environment.GetEnvironmentVariable("GMAIL_ADDRESS");
        var password = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD");
        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(password))
        {
            mailbox = null;
            hint = "";
            return false;
        }

        mailbox = new GmailMailbox(address.Trim(), password);
        hint = "";
        return true;
    }
}
