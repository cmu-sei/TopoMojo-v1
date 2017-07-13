using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TopoMojo.Extensions;
using TopoMojo.Models;
using MailKit.Net.Smtp;
using MailKit;
using MailKit.Security;
using MimeKit;

namespace TopoMojo.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public AuthMessageSender(
            ILogger<AuthMessageSender> logger,
            IOptions<ApplicationOptions> options
        ) {
            _logger = logger;
            _options = options.Value;
        }
        private readonly ILogger<AuthMessageSender> _logger;
        private readonly ApplicationOptions _options;

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            await Task.Delay(0);
            // Plug in your email service here to send an email.
            var message = new MimeMessage ();
            message.From.Add (new MailboxAddress (_options.Site.Name, _options.Site.Email.Sender));
            message.To.Add (new MailboxAddress (email, email));
            message.Subject = subject;

            var alt = new Multipart("alternative");
            alt.Add(new TextPart ("plain") { Text = body });
            alt.Add(new TextPart ("html") { Text = body });
            message.Body = alt;

            using (var client = new SmtpClient ()) {
                try
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    //client.ServerCertificateValidationCallback = (s,c,h,e) => true;

                    client.Connect(_options.Site.Email.Host, _options.Site.Email.Port); //, SecureSocketOptions.Auto);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove ("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    if (_options.Site.Email.User.HasValue() && _options.Site.Email.Password.HasValue())
                    {
                        client.Authenticate(_options.Site.Email.User, _options.Site.Email.Password);
                    }

                    client.Send (message);
                    client.Disconnect (true);
                    _logger.LogInformation($"Sent email to {email}");
                }
                catch
                {
                    _logger.LogError("Failed to send email host:{0}, address:{1}", _options.Site.Email.Host, email);
                }
            }
            //return Task.FromResult(0);
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
