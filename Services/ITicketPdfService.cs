using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Services
{
    public interface ITicketPdfService
    {
        Task<byte[]> GenerateBookingTicketsPdfAsync(Booking booking);
    }
}


