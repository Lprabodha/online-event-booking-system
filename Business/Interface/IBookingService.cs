using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;

namespace online_event_booking_system.Business.Interface
{
    public interface IBookingService
    {
        Task<CheckoutViewModel> GetCheckoutDataAsync(Guid eventId);
        Task<CheckoutResponse> ProcessCheckoutAsync(ProcessCheckoutRequest request, string userId);
        Task<bool> ValidateBookingAsync(ProcessCheckoutRequest request);
        Task<bool> ProcessPaymentAsync(string paymentIntentId, Guid bookingId);
        Task<Booking?> GetBookingByIdAsync(Guid bookingId);
        Task<List<Booking>> GetUserBookingsAsync(string userId);
        Task<List<Payment>> GetUserPaymentsAsync(string userId);
        Task<LoyaltyPoint?> GetUserLoyaltyPointsAsync(string userId);
        Task<bool> AddLoyaltyPointsAsync(string userId, int points, string description);
        Task<bool> CancelBookingAsync(Guid bookingId, string userId);
        Task<bool> RefundBookingAsync(Guid bookingId);
    }
}
