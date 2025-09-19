using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0")]
        public decimal Amount { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Discount must be greater than or equal to 0")]
        public decimal DiscountAmount { get; set; } = 0;
        
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; } = "CreditCard"; // CreditCard, PayPal, etc.
        
        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "LKR";

        [MaxLength(200)]
        public string? TransactionId { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Foreign Keys
        public string CustomerId { get; set; } = default!;
        public ApplicationUser Customer { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
