namespace online_event_booking_system.Models
{
    /// <summary>
    /// SMTP settings for email configuration
    /// </summary>
    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }
}
