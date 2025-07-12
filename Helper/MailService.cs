using System.Net.Mail;
using System.Net;
using LinkedInAPI.Models;
using Microsoft.Extensions.Options;

namespace LinkedInAPI.Helper
{
    public class MailService
    {
        private readonly MailSettings _mailSettings;
        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_mailSettings.Host, _mailSettings.Port)
            {
                Credentials = new NetworkCredential(_mailSettings.UserName, _mailSettings.Password),
                EnableSsl = _mailSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_mailSettings.FromEmail!),
                Subject = subject,
                Body = body,
                IsBodyHtml = false 
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
