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
				var doc = new Document(PageSize.A4, 28, 28, 28, 28);
				PdfWriter.GetInstance(doc, ms);
				doc.Open();

				// Fonts and colors
				var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, new BaseColor(255, 255, 255));
				var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(55, 65, 81));
				var labelFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(107, 114, 128));
				var valueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(31, 41, 55));
				var smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, new BaseColor(75, 85, 99));
				var darkBg = new BaseColor(30, 41, 59);
				var lightBg = new BaseColor(243, 244, 246);
				var accent = new BaseColor(99, 102, 241); // indigo

				// Document header (one time)
				var header = new PdfPTable(2) { WidthPercentage = 100 };
				header.SetWidths(new float[] { 70, 30 });
				var headerLeft = new PdfPCell(new Phrase($"{booking.Event.Title}", titleFont))
				{ BackgroundColor = darkBg, Border = Rectangle.NO_BORDER, Padding = 14 };
				header.AddCell(headerLeft);
				var headerRight = new PdfPCell(new Phrase($"Booking Ref: {booking.BookingReference}", FontFactory.GetFont(FontFactory.HELVETICA, 10, Font.NORMAL, new BaseColor(229, 231, 235))))
				{ BackgroundColor = darkBg, Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 14 };
				header.AddCell(headerRight);
				doc.Add(header);
				doc.Add(new Paragraph("\n"));

				var ticketsList = booking.Tickets?.ToList() ?? new List<Ticket>();
				for (int index = 0; index < ticketsList.Count; index++)
				{
					var ticket = ticketsList[index];

					// Outer card table
					var card = new PdfPTable(1) { WidthPercentage = 100 };
					var cardCell = new PdfPCell { BackgroundColor = new BaseColor(255, 255, 255), Padding = 14, BorderColor = new BaseColor(229, 231, 235), BorderWidth = 1f };

					// Content: two columns (details | QR)
					var content = new PdfPTable(2) { WidthPercentage = 100 };
					content.SetWidths(new float[] { 62, 38 });

					// Left details
					var details = new PdfPTable(2) { WidthPercentage = 100 };
					details.SetWidths(new float[] { 30, 70 });
					details.AddCell(InfoLabel("Ticket #", labelFont));
					details.AddCell(InfoValue(ticket.TicketNumber, valueFont));
					details.AddCell(InfoLabel("Name", labelFont));
					details.AddCell(InfoValue(booking.Customer?.FullName ?? booking.CustomerId, valueFont));
					details.AddCell(InfoLabel("Category", labelFont));
					details.AddCell(InfoValue(ticket.EventPrice?.Category ?? "General", valueFont));
					details.AddCell(InfoLabel("Price", labelFont));
					details.AddCell(InfoValue($"LKR {(ticket.EventPrice?.Price ?? 0):N2}", valueFont));
					details.AddCell(InfoLabel("Date & Time", labelFont));
					details.AddCell(InfoValue($"{booking.Event.EventDate:MMM dd, yyyy}  {booking.Event.EventTime:hh:mm tt}", valueFont));
					details.AddCell(InfoLabel("Venue", labelFont));
					details.AddCell(InfoValue(booking.Event.Venue?.Name ?? "-", valueFont));

					var detailsCell = new PdfPCell(details) { Border = Rectangle.NO_BORDER, PaddingRight = 10 };
					content.AddCell(detailsCell);

					// Right QR
					PdfPCell qrCell;
					try
					{
						if (!string.IsNullOrEmpty(ticket.QRCode))
						{
							var qr = Image.GetInstance(ticket.QRCode);
							qr.ScaleToFit(180f, 180f);
							qrCell = new PdfPCell(qr) { Border = Rectangle.BOX, BorderColor = new BaseColor(229, 231, 235), Padding = 8f, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
						}
						else
						{
							qrCell = new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER };
						}
					}
					catch
					{
						qrCell = new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER };
					}
					content.AddCell(qrCell);

					cardCell.AddElement(content);
					card.AddCell(cardCell);
					doc.Add(card);

					// Footer note
					var note = new PdfPTable(1) { WidthPercentage = 100 };
					var noteCell = new PdfPCell(new Phrase("Please present this ticket and a valid ID at entry. The QR code is unique and non-transferable.", smallFont))
					{ BackgroundColor = lightBg, Border = Rectangle.NO_BORDER, Padding = 10 };
					note.AddCell(noteCell);
					doc.Add(note);

					if (index < ticketsList.Count - 1)
					{
						doc.NewPage();
					}
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

		private static PdfPCell InfoLabel(string text, Font font)
		{
			return new PdfPCell(new Phrase(text, font)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6f };
		}

		private static PdfPCell InfoValue(string text, Font font)
		{
			return new PdfPCell(new Phrase(text, font)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6f };
		}
    }
}


