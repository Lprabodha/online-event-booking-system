namespace online_event_booking_system.Data.Entities
{
    public class EventPrice
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; } = default!;
        public string Category { get; set; } = default!;
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal Price { get; set; }
    }
}
