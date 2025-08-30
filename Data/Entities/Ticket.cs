namespace online_event_booking_system.Data.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = default!;
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        public string QRCode { get; set; } = default!;
        public bool IsPaid { get; set; } = false;
        public bool IsUsed { get; set; } = false;

        public string CustomerId { get; set; } = default!;
        public ApplicationUser Customer { get; set; } = default!;

        public int BookingId { get; set; }
        public Booking Booking { get; set; } = default!;

        public int EventId { get; set; }
        public Event Event { get; set; } = default!;

        public int EventPriceId { get; set; }
        public EventPrice EventPrice { get; set; } = default!;

        public int PaymentId { get; set; }
        public Payment Payment { get; set; } = default!;

    }
}
