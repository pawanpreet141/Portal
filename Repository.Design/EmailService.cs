using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Employee.BusinessLogic
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            var senderEmail =
                _configuration["EmailSettings:SenderEmail"];

            var senderName =
                _configuration["EmailSettings:SenderName"];

            var username =
                _configuration["EmailSettings:Username"];

            var password =
                _configuration["EmailSettings:Password"];

            var smtpServer =
                _configuration["EmailSettings:SmtpServer"];

            var port =
                _configuration.GetValue<int>("EmailSettings:Port");

            if (string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(smtpServer) ||
                port <= 0)
            {
                throw new InvalidOperationException(
                    "SMTP configuration is missing.");
            }

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(senderName, senderEmail));

            message.To.Add(
                MailboxAddress.Parse(toEmail));

            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                smtpServer,
                port,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                username,
                password);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);
        }
    }
}


