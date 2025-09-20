using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface
{
    public interface ICustomerPdfService
    {
        /// <summary>
        /// Generate PDF tickets for a booking
        /// </summary>
        /// <param name="booking"></param>
        /// <returns></returns>
        Task<byte[]> GenerateTicketsPdfAsync(Booking booking);
        /// <summary>
        /// Generate PDF invoice for a booking
        /// </summary>
        /// <param name="booking"></param>
        /// <returns></returns>
        Task<byte[]> GenerateInvoicePdfAsync(Booking booking);
    }
}


