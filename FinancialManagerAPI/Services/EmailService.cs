using FinancialManagerAPI.Services;
using MailKit.Net.Smtp;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _username = "financialmanager.api@gmail.com";
    private readonly string _password = "ibvu mbhh zuwu trdh";
    private readonly string _from = "financialmanager.api@gmail.com";

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Financial Manager API", _from));

        message.To.Add(new MailboxAddress("", to));

        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_smtpHost, _smtpPort, false);
            await client.AuthenticateAsync(_username, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
