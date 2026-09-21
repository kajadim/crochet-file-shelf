using backend.Options;
using backend.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace backend.Services.Implementation
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;

        public SmtpEmailSender(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailOptions.SenderName, _emailOptions.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailOptions.SmtpHost, _emailOptions.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailOptions.Username, _emailOptions.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
