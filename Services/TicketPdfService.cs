using online_event_booking_system.Data.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace online_event_booking_system.Services
{
    public class TicketPdfService : ITicketPdfService
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly ILogger<TicketPdfService> _logger;

        public TicketPdfService(IQRCodeService qrCodeService, ILogger<TicketPdfService> logger)
        {
            _qrCodeService = qrCodeService;
            _logger = logger;
        }

        public async Task<byte[]> GenerateBookingTicketsPdfAsync(Booking booking)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Content().Column(column =>
                        {
                            column.Spacing(20);

                            column.Item().Text(text =>
                            {
                                text.Span("Star Events").FontSize(22).SemiBold();
                            });

                            column.Item().Text(text =>
                            {
                                text.Span($"Event: {booking.Event.Title}").FontSize(16).SemiBold();
                            });

                            column.Item().Text(text =>
                            {
                                text.Span($"Date: {booking.Event.EventDate:dddd, MMMM dd, yyyy}");
                            });

                            column.Item().Text(text =>
                            {
                                text.Span($"Venue: {booking.Event.Venue?.Name ?? "TBA"}");
                            });

                            // Tickets, one page per ticket
                            foreach (var ticket in booking.Tickets)
                            {
                                column.Item().PageBreak();

                                column.Item().Text($"Ticket #{ticket.TicketNumber}").FontSize(18).SemiBold();

                                column.Item().Row(row =>
                                {
                                    row.Spacing(20);

                                    row.RelativeItem().Column(info =>
                                    {
                                        info.Spacing(8);
                                        info.Item().Text($"Category: {ticket.EventPrice?.Category ?? "General Admission"}");
                                        info.Item().Text($"Price: LKR {ticket.EventPrice?.Price.ToString("F2") ?? "0.00"}");
                                        info.Item().Text($"Booking Ref: {booking.BookingReference}");
                                        info.Item().Text($"Buyer: {booking.Customer.FullName}");
                                    });

                                    // QR Code
                                    var qrBytes = _qrCodeService.GenerateTicketQRCodeBytes(
                                        ticket.Id, booking.EventId, booking.CustomerId, ticket.TicketNumber, 300);

                                    row.ConstantItem(200).Image(qrBytes);
                                });

                                column.Item().Text("Please present this QR code at the event entrance.")
                                    .FontSize(10).FontColor(Colors.Grey.Medium);
                            }
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tickets PDF for booking {BookingId}", booking.Id);
                return Array.Empty<byte>();
            }
        }
    }
}


