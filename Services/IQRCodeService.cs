namespace online_event_booking_system.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCode(string data, int size = 200);
        byte[] GenerateQRCodeBytes(string data, int size = 200);
        string GenerateTicketQRCode(Guid ticketId, Guid eventId, string customerId);
        string GenerateTicketQRCodeWithData(Guid ticketId, Guid eventId, string customerId, string ticketNumber);
        byte[] GenerateTicketQRCodeBytes(Guid ticketId, Guid eventId, string customerId, string ticketNumber, int size = 256);
    }
}
