using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface
{
    public interface ICustomerPdfService
    {
        Task<byte[]> GenerateTicketsPdfAsync(Booking booking);
        Task<byte[]> GenerateInvoicePdfAsync(Booking booking);
    }
}


