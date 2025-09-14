using QRCoder;
using System.Text;
using System.IO;

namespace online_event_booking_system.Services
{
    public class QRCodeService : IQRCodeService
    {
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
                // Log error and return empty string
                Console.WriteLine($"Error generating QR code: {ex.Message}");
                return string.Empty;
            }
        }

        public string GenerateTicketQRCode(Guid ticketId, Guid eventId, string customerId)
        {
            // Create a structured data string for the ticket
            var ticketData = new
            {
                ticketId = ticketId.ToString(),
                eventId = eventId.ToString(),
                customerId = customerId,
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                type = "ticket"
            };

            var jsonData = System.Text.Json.JsonSerializer.Serialize(ticketData);
            return GenerateQRCode(jsonData);
        }
    }
}
