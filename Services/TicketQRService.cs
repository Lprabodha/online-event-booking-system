using online_event_booking_system.Helper;

namespace online_event_booking_system.Services
{
    public class TicketQRService : ITicketQRService
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly IS3Service _s3Service;
        private readonly IEmailService _emailService;
        private readonly ILogger<TicketQRService> _logger;

        public TicketQRService(
            IQRCodeService qrCodeService,
            IS3Service s3Service,
            IEmailService emailService,
            ILogger<TicketQRService> logger)
        {
            _qrCodeService = qrCodeService;
            _s3Service = s3Service;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> GenerateAndUploadTicketQRCodeAsync(
            Guid ticketId, 
            Guid eventId, 
            string customerId, 
            string ticketNumber, 
            string customerEmail, 
            string customerName, 
            string eventName, 
            DateTime eventDate, 
            string venueName)
        {
            try
            {
                // Generate QR code as byte array
                var qrCodeBytes = _qrCodeService.GenerateTicketQRCodeBytes(
                    ticketId, 
                    eventId, 
                    customerId, 
                    ticketNumber, 
                    300); // Higher resolution for better quality

                if (qrCodeBytes == null || qrCodeBytes.Length == 0)
                {
                    _logger.LogError("Failed to generate QR code for ticket {TicketId}", ticketId);
                    throw new InvalidOperationException("Failed to generate QR code");
                }

                // Create filename for QR code
                var fileName = $"ticket_{ticketNumber}_{ticketId}.png";
                
                // Upload QR code to S3
                var qrCodePath = await _s3Service.UploadByteArrayAsync(
                    qrCodeBytes, 
                    fileName, 
                    "image/png", 
                    "tickets/qr-codes");

                _logger.LogInformation("QR code uploaded successfully for ticket {TicketId} at path {Path}", ticketId, qrCodePath);

                // Send email with QR code attachment
                await SendTicketEmailWithQRCodeAsync(
                    customerEmail, 
                    customerName, 
                    eventName, 
                    eventDate, 
                    venueName, 
                    ticketNumber, 
                    qrCodePath);

                return qrCodePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and uploading QR code for ticket {TicketId}", ticketId);
                throw;
            }
        }

        public async Task<string> GetQRCodeUrlAsync(string qrCodePath)
        {
            try
            {
                // Use direct URL for better performance (no async S3 calls)
                return _s3Service.GetDirectUrl(qrCodePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting QR code URL for path {Path}", qrCodePath);
                return qrCodePath; // Return the path as fallback
            }
        }

        public async Task<bool> SendTicketEmailWithQRCodeAsync(
            string customerEmail, 
            string customerName, 
            string eventName, 
            DateTime eventDate, 
            string venueName, 
            string ticketNumber, 
            string qrCodePath)
        {
            try
            {
                // Get QR code URL for email template
                var qrCodeUrl = await GetQRCodeUrlAsync(qrCodePath);

                // Create email template
                var emailBody = CreateTicketEmailTemplate(
                    customerName, 
                    eventName, 
                    eventDate, 
                    venueName, 
                    ticketNumber, 
                    qrCodeUrl);

                // Send email
                await _emailService.SendEmailAsync(
                    customerEmail,
                    $"Your Ticket for {eventName} - Star Events",
                    emailBody);

                _logger.LogInformation("Ticket email sent successfully to {Email} for ticket {TicketNumber}", customerEmail, ticketNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket email to {Email} for ticket {TicketNumber}", customerEmail, ticketNumber);
                return false;
            }
        }

        private string CreateTicketEmailTemplate(
            string customerName, 
            string eventName, 
            DateTime eventDate, 
            string venueName, 
            string ticketNumber, 
            string qrCodeUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Your Event Ticket - Star Events</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f8f9fa;
        }}
        .container {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 20px;
            padding: 40px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 32px;
            font-weight: bold;
            color: white;
            margin-bottom: 10px;
        }}
        .tagline {{
            color: rgba(255,255,255,0.8);
            font-size: 14px;
        }}
        .content {{
            background: white;
            border-radius: 15px;
            padding: 30px;
            margin-bottom: 20px;
        }}
        .greeting {{
            font-size: 24px;
            font-weight: 600;
            color: #2d3748;
            margin-bottom: 20px;
        }}
        .ticket-info {{
            background: #f7fafc;
            border-left: 4px solid #4299e1;
            padding: 20px;
            margin: 20px 0;
            border-radius: 8px;
        }}
        .ticket-info h3 {{
            color: #2b6cb0;
            margin: 0 0 15px 0;
            font-size: 18px;
        }}
        .ticket-detail {{
            display: flex;
            justify-content: space-between;
            margin: 10px 0;
            padding: 8px 0;
            border-bottom: 1px solid #e2e8f0;
        }}
        .ticket-detail:last-child {{
            border-bottom: none;
        }}
        .ticket-detail strong {{
            color: #2d3748;
        }}
        .ticket-detail span {{
            color: #4a5568;
        }}
        .qr-section {{
            text-align: center;
            margin: 30px 0;
            padding: 20px;
            background: #f7fafc;
            border-radius: 10px;
        }}
        .qr-section h3 {{
            color: #2b6cb0;
            margin-bottom: 15px;
        }}
        .qr-code {{
            max-width: 200px;
            height: auto;
            border: 3px solid #4299e1;
            border-radius: 10px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }}
        .instructions {{
            background: #e8f5e9;
            border-left: 4px solid #4caf50;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .instructions h4 {{
            color: #1b5e20;
            margin: 0 0 10px 0;
            font-size: 14px;
        }}
        .instructions p {{
            margin: 0;
            font-size: 13px;
            color: #2e7d32;
        }}
        .footer {{
            text-align: center;
            color: rgba(255,255,255,0.8);
            font-size: 12px;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🎫 Star Events</div>
            <div class='tagline'>Where Every Event Becomes a Memory</div>
        </div>
        
        <div class='content'>
            <div class='greeting'>Hello {customerName}!</div>
            
            <p>Your ticket has been successfully generated! We're excited to see you at the event.</p>
            
            <div class='ticket-info'>
                <h3>🎟️ Ticket Information</h3>
                <div class='ticket-detail'>
                    <strong>Event:</strong>
                    <span>{eventName}</span>
                </div>
                <div class='ticket-detail'>
                    <strong>Date & Time:</strong>
                    <span>{eventDate:dddd, MMMM dd, yyyy 'at' h:mm tt}</span>
                </div>
                <div class='ticket-detail'>
                    <strong>Venue:</strong>
                    <span>{venueName}</span>
                </div>
                <div class='ticket-detail'>
                    <strong>Ticket Number:</strong>
                    <span>{ticketNumber}</span>
                </div>
            </div>
            
            <div class='qr-section'>
                <h3>📱 Your QR Code</h3>
                <p>Present this QR code at the event entrance for quick check-in:</p>
                <img src='{qrCodeUrl}' alt='Ticket QR Code' class='qr-code'>
            </div>
            
            <div class='instructions'>
                <h4>📋 Important Instructions</h4>
                <p>
                    • Please arrive 15-30 minutes before the event starts<br>
                    • Bring a valid ID for verification<br>
                    • Keep this email and QR code accessible on your phone<br>
                    • The QR code is unique to your ticket and cannot be transferred
                </p>
            </div>
            
            <p style='text-align: center; margin-top: 30px;'>
                <strong>Thank you for choosing Star Events!</strong><br>
                We look forward to providing you with an amazing experience.
            </p>
        </div>
        
        <div class='footer'>
            <p>This is your official ticket confirmation. Please keep this email safe.</p>
            <p>&copy; {DateTime.Now.Year} Star Events. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
