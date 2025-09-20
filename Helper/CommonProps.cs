namespace online_event_booking_system.Helper
{
    /// <summary>
    /// Common properties for entities.
    /// </summary>
    public class CommonProps
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
