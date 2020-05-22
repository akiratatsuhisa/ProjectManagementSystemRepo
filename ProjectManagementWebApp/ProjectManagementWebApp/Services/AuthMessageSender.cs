using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using ProjectManagementWebApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Services
{
    public class AuthMessageSender : IEmailSender
    {
        private EmailSettings _emailSettings { get; }

        public AuthMessageSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Task.FromResult(0);
        }

        public async Task Execute(string email, string subject, string message)
        {
            try
            {
                string toEmail = string.IsNullOrEmpty(email)
                                 ? _emailSettings.ToEmail
                                 : email;
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.Username, "Hutech")
                };
                mail.To.Add(new MailAddress(toEmail));
                mail.CC.Add(new MailAddress(_emailSettings.CcEmail));

                mail.Subject = $"Hutech - {subject}";
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                using (SmtpClient smtp = new SmtpClient(_emailSettings.Domain, _emailSettings.Port))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message, exception.InnerException);
            }
        }
    }
}
