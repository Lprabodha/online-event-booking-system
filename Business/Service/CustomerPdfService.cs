using iTextSharp.text;
using iTextSharp.text.pdf;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Service
{
    public class CustomerPdfService : ICustomerPdfService
    {
        public async Task<byte[]> GenerateTicketsPdfAsync(Booking booking)
        {
            return await Task.Run(() =>
            {
                using var ms = new MemoryStream();
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                doc.Add(new Paragraph($"Tickets - {booking.Event.Title}", titleFont));
                doc.Add(new Paragraph($"Booking Ref: {booking.BookingReference}", textFont));
                doc.Add(new Paragraph("\n"));

                foreach (var ticket in booking.Tickets)
                {
                    // Ticket header
                    var table = new PdfPTable(2) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 60, 40 });

                    var left = new PdfPTable(1) { WidthPercentage = 100 };
                    left.AddCell(Cell($"Ticket #: {ticket.TicketNumber}", textFont));
                    left.AddCell(Cell($"Category: {ticket.EventPrice?.Category ?? "General"}", textFont));
                    left.AddCell(Cell($"Price: {ticket.EventPrice?.Price.ToString("F2") ?? "0.00"}", textFont));
                    left.AddCell(Cell($"Event Date: {booking.Event.EventDate:MMM dd, yyyy}", textFont));
                    left.AddCell(Cell($"Venue: {booking.Event.Venue?.Name}", textFont));

                    var leftCell = new PdfPCell(left) { Border = Rectangle.NO_BORDER };
                    table.AddCell(leftCell);

                    // QR code image (if path is an URL, we can load it directly)
                    try
                    {
                        if (!string.IsNullOrEmpty(ticket.QRCode))
                        {
                            var img = Image.GetInstance(ticket.QRCode);
                            img.ScaleToFit(140f, 140f);
                            var imgCell = new PdfPCell(img) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
                            table.AddCell(imgCell);
                        }
                        else
                        {
                            table.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                        }
                    }
                    catch
                    {
                        table.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                    }

                    doc.Add(table);
                    doc.Add(new Paragraph("\n"));
                    doc.NewPage();
                }

                doc.Close();
                return ms.ToArray();
            });
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(Booking booking)
        {
            return await Task.Run(() =>
            {
                using var ms = new MemoryStream();
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                doc.Add(new Paragraph("Payment Invoice", titleFont));
                doc.Add(new Paragraph($"Invoice #: {booking.Id.ToString().Substring(0,8).ToUpper()}", textFont));
                doc.Add(new Paragraph($"Date: {DateTime.UtcNow:MMM dd, yyyy}", textFont));
                doc.Add(new Paragraph("\n"));

                // Bill to
                doc.Add(new Paragraph("Bill To", headerFont));
                doc.Add(new Paragraph(booking.Customer?.FullName ?? booking.CustomerId, textFont));
                doc.Add(new Paragraph(booking.Customer?.Email ?? "", textFont));
                doc.Add(new Paragraph(booking.Customer?.Address ?? "", textFont));
                doc.Add(new Paragraph("\n"));

                // Event info
                doc.Add(new Paragraph("Event", headerFont));
                doc.Add(new Paragraph(booking.Event.Title, textFont));
                doc.Add(new Paragraph($"{booking.Event.EventDate:MMM dd, yyyy} - {booking.Event.Venue?.Name}", textFont));
                doc.Add(new Paragraph("\n"));

                // Line items
                var table = new PdfPTable(4) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 50, 15, 15, 20 });
                table.AddCell(Header("Description", headerFont));
                table.AddCell(Header("Qty", headerFont));
                table.AddCell(Header("Price", headerFont));
                table.AddCell(Header("Total", headerFont));

                decimal subtotal = 0;
                foreach (var t in booking.Tickets)
                {
                    var desc = t.EventPrice?.Description ?? t.EventPrice?.Category ?? "Ticket";
                    var price = t.EventPrice?.Price ?? 0;
                    table.AddCell(Cell(desc, textFont));
                    table.AddCell(Cell("1", textFont));
                    table.AddCell(Cell(price.ToString("F2"), textFont));
                    table.AddCell(Cell(price.ToString("F2"), textFont));
                    subtotal += price;
                }

                doc.Add(table);
                doc.Add(new Paragraph("\n"));

                // Totals
                var totals = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
                totals.AddCell(Cell("Subtotal:", headerFont));
                totals.AddCell(Cell(subtotal.ToString("F2"), textFont));
                totals.AddCell(Cell("Total:", headerFont));
                totals.AddCell(Cell(subtotal.ToString("F2"), textFont));
                doc.Add(totals);

                doc.Close();
                return ms.ToArray();
            });
        }

        private static PdfPCell Cell(string text, Font font)
        {
            return new PdfPCell(new Phrase(text, font)) { Border = Rectangle.NO_BORDER, Padding = 4 };
        }

        private static PdfPCell Header(string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font)) { Padding = 6 };
            cell.BackgroundColor = new BaseColor(240, 240, 240);
            return cell;
        }
    }
}


