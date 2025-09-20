namespace online_event_booking_system.Models
{
    /// <summary>
    /// AWS settings for S3 integration
    /// </summary>
    public class AwsSettings
    {
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = "us-east-1";
        public string S3BucketName { get; set; } = "event-booking-images";
    }
}
