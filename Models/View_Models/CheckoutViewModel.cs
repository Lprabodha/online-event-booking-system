using System.ComponentModel.DataAnnotations;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models.View_Models
{
    public class CheckoutViewModel
    {
        public Guid EventId { get; set; }
        public Event Event { get; set; } = default!;
        public List<CheckoutTicketItem> TicketItems { get; set; } = new();
        public string? DiscountCode { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal Total { get; set; }
        public string? StripePublishableKey { get; set; }
        public int AvailableLoyaltyPoints { get; set; }
    }

    public class CheckoutTicketItem
    {
        public Guid EventPriceId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }
        public int AvailableStock { get; set; }
        public decimal Subtotal => Price * Quantity;
    }

    public class ProcessCheckoutRequest
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public List<TicketPurchase> Tickets { get; set; } = new();

        public string? DiscountCode { get; set; }

        [Required]
        public string PaymentMethodId { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Range(0, int.MaxValue)]
        public int RedeemPoints { get; set; } = 0;
    }

    public class TicketPurchase
    {
        [Required]
        public Guid EventPriceId { get; set; }

        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10")]
        public int Quantity { get; set; }
    }

    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        public Guid? BookingId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
