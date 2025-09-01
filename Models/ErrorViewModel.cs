namespace OnlineBookstore.Models
{
    // Represents error details for displaying on an error page
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
