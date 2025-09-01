namespace OnlineBookstore.Utilities
{
    // Defines an email-sending contract for the application
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default);
    }
}
