using FootballSquad.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace FootballSquad.Core.Services
{
    public class GmailEmailService : IEmailService
    {
        const string GMAIL_SMTP_SERVER = "smtp.gmail.com";
        const int GMAIL_SMTP_PORT = 587;
        readonly string GMAIL_SMTP_EMAIL;
        readonly string GMAIL_SMTP_PASSWORD;
        readonly string REACT_APP_DOMAIN;

        public GmailEmailService(IConfiguration _configuration)
        {
            GMAIL_SMTP_EMAIL = _configuration["Gmail_Email"];
            GMAIL_SMTP_PASSWORD = _configuration["Gmail_Password"];
            REACT_APP_DOMAIN = _configuration["ReactAppDomain"];
        }

        public void SendForgotPasswordEmail(string email, string token)
        {
            using (var client = new SmtpClient(GMAIL_SMTP_SERVER, GMAIL_SMTP_PORT))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(GMAIL_SMTP_EMAIL, GMAIL_SMTP_PASSWORD);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(GMAIL_SMTP_EMAIL),
                    Subject = "FootballSquad - Recover Password",
                    Body = "<html><body>" +
                    "<h1>If you are not requested to recover password ignore this email.</h1>" +
                        $"<div style=\"background-color: #eeeeee\">" +
                        $"Click this " +
                        $"<a href=\"{REACT_APP_DOMAIN}/reset_password?email={email}&token={token}\">link</a>" +
                        $" to set a new password for your account" +
                        "</div></body></html>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);
                client.Send(mailMessage);
            }
        }
    }
}