using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required, MaxLength(50)]
        public string TicketNumber { get; set; } = default!;
        
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        
        [Required, MaxLength(500)]
        public string QRCode { get; set; } = default!; // S3 path or local path to QR code image
        
        public bool IsPaid { get; set; } = false;
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }

        // Foreign Keys
        public string CustomerId { get; set; } = default!;
        public ApplicationUser Customer { get; set; } = default!;

        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = default!;

        public Guid EventId { get; set; }
        public Event Event { get; set; } = default!;

        public Guid EventPriceId { get; set; }
        public EventPrice EventPrice { get; set; } = default!;

        public Guid PaymentId { get; set; }
        public Payment Payment { get; set; } = default!;
    }
}
