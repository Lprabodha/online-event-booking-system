namespace online_event_booking_system.Services
{
    public interface ITicketQRService
    {
        Task<string> GenerateAndUploadTicketQRCodeAsync(Guid ticketId, Guid eventId, string customerId, string ticketNumber, string customerEmail, string customerName, string eventName, DateTime eventDate, string venueName);
        Task<string> GetQRCodeUrlAsync(string qrCodePath);
        Task<bool> SendTicketEmailWithQRCodeAsync(string customerEmail, string customerName, string eventName, DateTime eventDate, string venueName, string ticketNumber, string qrCodePath);
    }
}
