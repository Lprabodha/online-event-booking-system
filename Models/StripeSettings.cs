namespace online_event_booking_system.Models
{
    /// <summary>
    /// Configuration settings for Stripe payment gateway.
    /// </summary>
    public class StripeSettings
    {
        public string PublishableKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
