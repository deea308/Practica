using System.Net;
using System.Net.Mail;

namespace OnlineBookstore.Utilities
{
    // Sends emails using SMTP (configured via appsettings.json)
    public class SmtpEmailSender : IEmailSender, IDisposable
    {
        private readonly SmtpClient _client;
        private readonly string _from;

        public SmtpEmailSender(IConfiguration cfg)
        {
            var s = cfg.GetSection("Smtp");
            _from = s["From"] ?? throw new InvalidOperationException("Smtp:From is required");
            var host = s["Host"] ?? throw new InvalidOperationException("Smtp:Host is required");
            var port = int.Parse(s["Port"] ?? "587");
            var useSsl = bool.Parse(s["UseSsl"] ?? "true");
            var user = s["User"];
            var pass = s["Pass"];

            _client = new SmtpClient(host, port)
            {
                EnableSsl = useSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(user))
            {
                _client.Credentials = new NetworkCredential(user, pass);
            }
        }

        public Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
        {
            var msg = new MailMessage
            {
                From = new MailAddress(_from),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(new MailAddress(to));

            if (!string.IsNullOrWhiteSpace(textBody))
            {
                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));
            }

            return _client.SendMailAsync(msg);
        }

        public void Dispose() => _client?.Dispose();
    }
}
