using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        public string PaymentMethod { get; set; } = "CreditCard"; // CreditCard, PayPal, etc.
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        [MaxLength(50)]
        public string Currency { get; set; } = "USD";

        public string CustomerId { get; set; } = default!;
        public ApplicationUser Customer { get; set; } = default!;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
