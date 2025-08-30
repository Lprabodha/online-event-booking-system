namespace online_event_booking_system.Data.Entities
{
    public class Discount
    {
        public int Id { get; set; }
        public int? EventId { get; set; }
        public Event? Event { get; set; }
        public string Code { get; set; } = default!;
        public string Type { get; set; } = "Percent"; // Percent, Amount
        public decimal Value { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
