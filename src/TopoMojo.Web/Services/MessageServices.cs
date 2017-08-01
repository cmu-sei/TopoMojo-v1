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
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }

    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public AuthMessageSender(
            ILogger<AuthMessageSender> logger,
            ControlOptions branding,
            MessagingOptions options
        ) {
            _logger = logger;
            _options = options;
            _branding = branding;
        }
        private readonly ILogger<AuthMessageSender> _logger;
        private readonly MessagingOptions _options;
        private readonly ControlOptions _branding;

        public Task SendEmailAsync(string email, string subject, string body)
        {
            var message = new MimeMessage ();
            message.From.Add (new MailboxAddress (_branding.ApplicationName, _options.Email.Sender));
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

                    client.Connect(_options.Email.Host, _options.Email.Port); //, SecureSocketOptions.Auto);
                    client.AuthenticationMechanisms.Remove ("XOAUTH2");

                    if (_options.Email.User.HasValue() && _options.Email.Password.HasValue())
                    {
                        client.Authenticate(_options.Email.User, _options.Email.Password);
                    }

                    client.Send (message);
                    client.Disconnect (true);
                    _logger.LogInformation($"Sent email to {email}");
                }
                catch
                {
                    _logger.LogError("Failed to send email host:{0}, address:{1}", _options.Email.Host, email);
                }
            }
            return Task.FromResult(0);
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
