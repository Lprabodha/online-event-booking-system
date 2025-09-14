using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Services
{
    public interface IPaymentService
    {
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, string eventId);
        Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId);
        Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
        Task<Customer> CreateOrGetStripeCustomerAsync(ApplicationUser user);
        Task<bool> ValidatePaymentAsync(string paymentIntentId);
    }

    public class PaymentIntent
    {
        public string Id { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }

    public class Customer
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
