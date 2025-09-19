namespace online_event_booking_system.Services
{
    /// <summary>
    /// Interface for email service to send emails.
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachmentData, string attachmentFileName, string attachmentContentType);
        Task SendRefundEmailAsync(string toEmail, string customerName, string eventTitle, DateTime eventDate, decimal amount, string bookingReference);
    }
}
