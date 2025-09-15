using QRCoder;
using System.Text;
using System.IO;

namespace online_event_booking_system.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(ILogger<QRCodeService> logger)
        {
            _logger = logger;
        }

        public string GenerateQRCode(string data, int size = 200)
        {
            try
            {
                using var qrGenerator = new QRCoder.QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(data, QRCoder.QRCodeGenerator.ECCLevel.Q);

                // Use PngByteQRCode renderer which returns PNG bytes
                using var pngRenderer = new QRCoder.PngByteQRCode(qrCodeData);
                var imageBytes = pngRenderer.GetGraphic(size);
                return "data:image/png;base64," + Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return string.Empty;
            }
        }

        public byte[] GenerateQRCodeBytes(string data, int size = 200)
        {
            try
            {
                using var qrGenerator = new QRCoder.QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(data, QRCoder.QRCodeGenerator.ECCLevel.Q);

                // Use PngByteQRCode renderer which returns PNG bytes
                using var pngRenderer = new QRCoder.PngByteQRCode(qrCodeData);
                return pngRenderer.GetGraphic(size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code bytes");
                return Array.Empty<byte>();
            }
        }

        public string GenerateTicketQRCode(Guid ticketId, Guid eventId, string customerId)
        {
            try
            {
                // Create a structured data string for the ticket (no API URLs)
                var ticketData = new
                {
                    ticketId = ticketId.ToString(),
                    eventId = eventId.ToString(),
                    customerId = customerId,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    type = "ticket",
                    version = "1.0"
                };

                var jsonData = System.Text.Json.JsonSerializer.Serialize(ticketData);
                return GenerateQRCode(jsonData, 256); // Larger size for better scanning
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating ticket QR code for ticket {TicketId}", ticketId);
                return string.Empty;
            }
        }

        public string GenerateTicketQRCodeWithData(Guid ticketId, Guid eventId, string customerId, string ticketNumber)
        {
            try
            {
                // Create a simple, readable QR code data format
                var qrData = $"TICKET:{ticketNumber}|EVENT:{eventId}|CUSTOMER:{customerId}|ID:{ticketId}";
                return GenerateQRCode(qrData, 256);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating detailed ticket QR code for ticket {TicketId}", ticketId);
                return string.Empty;
            }
        }

        public byte[] GenerateTicketQRCodeBytes(Guid ticketId, Guid eventId, string customerId, string ticketNumber, int size = 256)
        {
            try
            {
                // Create a simple, readable QR code data format
                var qrData = $"TICKET:{ticketNumber}|EVENT:{eventId}|CUSTOMER:{customerId}|ID:{ticketId}";
                return GenerateQRCodeBytes(qrData, size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating ticket QR code bytes for ticket {TicketId}", ticketId);
                return Array.Empty<byte>();
            }
        }
    }
}
