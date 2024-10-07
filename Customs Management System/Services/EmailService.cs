using MimeKit;
using System.Net.Mail;
using MailKit.Net.Smtp;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Customs_Management_System.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject=subject;
       

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body =builder.ToMessageBody();

            using var smtp = new SmtpClient();

            smtp.Connect(_configuration["EmailSettings:SmtpServer"], int.Parse( _configuration["EmailSettings:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderPassword"]);

            await smtp.SendAsync(email);
            smtp.Disconnect(true);

        }
    }
}
