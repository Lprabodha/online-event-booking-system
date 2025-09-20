namespace online_event_booking_system.Services
{
    /// <summary>
    /// Service interface for generating, uploading, and emailing ticket QR codes.
    /// </summary>
    public interface ITicketQRService
    {
        /// <summary>
        /// Generate a QR code for the ticket, upload it to cloud storage, and return the URL.
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="eventId"></param>
        /// <param name="customerId"></param>
        /// <param name="ticketNumber"></param>
        /// <param name="customerEmail"></param>
        /// <param name="customerName"></param>
        /// <param name="eventName"></param>
        /// <param name="eventDate"></param>
        /// <param name="venueName"></param>
        /// <returns></returns>
        Task<string> GenerateAndUploadTicketQRCodeAsync(Guid ticketId, Guid eventId, string customerId, string ticketNumber, string customerEmail, string customerName, string eventName, DateTime eventDate, string venueName);
        /// <summary>
        /// Get the URL of the QR code image stored in cloud storage.
        /// </summary>
        /// <param name="qrCodePath"></param>
        /// <returns></returns>
        Task<string> GetQRCodeUrlAsync(string qrCodePath);
        /// <summary>
        /// Send an email to the customer with the ticket details and embedded QR code.
        /// </summary>
        /// <param name="customerEmail"></param>
        /// <param name="customerName"></param>
        /// <param name="eventName"></param>
        /// <param name="eventDate"></param>
        /// <param name="venueName"></param>
        /// <param name="ticketNumber"></param>
        /// <param name="qrCodePath"></param>
        /// <returns></returns>
        Task<bool> SendTicketEmailWithQRCodeAsync(string customerEmail, string customerName, string eventName, DateTime eventDate, string venueName, string ticketNumber, string qrCodePath);
    }
}
