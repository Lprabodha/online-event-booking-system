namespace online_event_booking_system.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCode(string data, int size = 200);
        string GenerateTicketQRCode(Guid ticketId, Guid eventId, string customerId);
    }
}
