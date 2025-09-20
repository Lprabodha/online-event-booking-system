using ClosedXML.Excel;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Models;
using online_event_booking_system.Repository.Interface;
using online_event_booking_system.Data.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;

namespace online_event_booking_system.Business.Service
{
    /// <summary>
    /// Service class for report-related operations, implementing IReportService interface.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        /// <summary>
        /// Generate report based on the specified parameters
        /// </summary>
        /// <param name="reportType"></param>
        /// <param name="format"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<byte[]> GenerateReportAsync(string reportType, string format, DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null, string role = null)
        {
            switch (reportType.ToLower())
            {
                case "events":
                    var events = await _reportRepository.GetEventsAsync(dateFrom, dateTo, category, organizer);
                    return GenerateEventReport(events, format, dateFrom, dateTo, category, organizer);
                case "users":
                    var users = await _reportRepository.GetUsersAsync(dateFrom, dateTo, role);
                    return GenerateUserReport(users, format, dateFrom, dateTo, role);
                case "organizers":
                    var organizers = await _reportRepository.GetOrganizersAsync(dateFrom, dateTo);
                    return GenerateFile(organizers, format, "Organizers");
                case "customers":
                    var customers = await _reportRepository.GetCustomersAsync(dateFrom, dateTo);
                    return GenerateFile(customers, format, "Customers");
                default:
                    throw new NotImplementedException($"Report type '{reportType}' is not supported.");
            }
        }

        /// <summary>
        /// Get users based on the specified parameters
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync(DateTime? dateFrom, DateTime? dateTo, string role = null)
        {
            return await _reportRepository.GetUsersAsync(dateFrom, dateTo, role);
        }

        /// <summary>
        /// Get events based on the specified parameters
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Event>> GetEventsAsync(DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null)
        {
            return await _reportRepository.GetEventsAsync(dateFrom, dateTo, category, organizer);
        }

        /// <summary>
        /// Get recent reports generated within the specified date range
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RecentReport>> GetRecentReportsAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            var allReports = new List<RecentReport>();

            var events = await _reportRepository.GetEventsAsync(dateFrom, dateTo);
            allReports.AddRange(events.Select(e => new RecentReport
            {
                ReportName = $"Event Report - {e.Title}",
                Type = "Events",
                Format = "PDF",
                Generated = e.CreatedAt,
                FileName = $"event_{e.Id}.pdf"
            }));

            var users = await _reportRepository.GetUsersAsync(dateFrom, dateTo);
            allReports.AddRange(users.Select(u => new RecentReport
            {
                ReportName = $"User Report - {u.FullName}",
                Type = "Users",
                Format = "Excel",
                Generated = u.CreatedAt,
                FileName = $"user_{u.Id}.xlsx"
            }));

            return allReports.OrderByDescending(r => r.Generated);
        }

        // Generate User Report with specific columns and formatting
        private byte[] GenerateUserReport(IEnumerable<ApplicationUser> users, string format, DateTime? dateFrom, DateTime? dateTo, string role)
        {
            var userData = users.Select(u => new UserReportData
            {
                FullName = u.FullName ?? "",
                Username = u.UserName ?? "",
                Email = u.Email ?? "",
                NIC = u.NIC ?? "",
                Address = u.Address ?? "",
                Status = u.IsActive ? "Active" : "Inactive",
                CreatedAt = u.CreatedAt.ToString("MMM dd, yyyy"),
                Role = role ?? "All Roles"
            }).ToList();

            var filterDetails = $"Date Range: {(dateFrom?.ToString("MMM dd, yyyy") ?? "All")} - {(dateTo?.ToString("MMM dd, yyyy") ?? "All")} | Role: {role ?? "All"}";

            if (format.ToLower() == "excel")
            {
                return GenerateUserExcel(userData, filterDetails);
            }
            else if (format.ToLower() == "pdf")
            {
                return GenerateUserPdf(userData, filterDetails);
            }
            else
            {
                return GenerateFile(userData, format, "Users");
            }
        }

        /// <summary>
        /// Generate Event Report with specific columns and formatting
        /// </summary>
        /// <param name="events"></param>
        /// <param name="format"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <returns></returns>
        private byte[] GenerateEventReport(IEnumerable<Event> events, string format, DateTime? dateFrom, DateTime? dateTo, string category, string organizer)
        {
            var eventData = events.Select(e => new EventReportData
            {
                EventTitle = e.Title ?? "",
                Category = e.Category?.Name ?? "",
                Organizer = e.Organizer?.FullName ?? "",
                EventDate = e.EventDate.ToString("MMM dd, yyyy"),
                Capacity = e.TotalCapacity,
                Status = e.Status?.ToString() ?? "",
                CreatedDate = e.CreatedAt.ToString("MMM dd, yyyy")
            }).ToList();

            var filterDetails = $"Date Range: {(dateFrom?.ToString("MMM dd, yyyy") ?? "All")} - {(dateTo?.ToString("MMM dd, yyyy") ?? "All")} | Category: {category ?? "All"} | Organizer: {organizer ?? "All"}";

            if (format.ToLower() == "excel")
            {
                return GenerateEventExcel(eventData, filterDetails);
            }
            else if (format.ToLower() == "pdf")
            {
                return GenerateEventPdf(eventData, filterDetails);
            }
            else
            {
                return GenerateFile(eventData, format, "Events");
            }
        }

        /// <summary>
        /// Generic file generator for different formats
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="format"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private byte[] GenerateFile<T>(IEnumerable<T> data, string format, string sheetName)
        {
            if (format.ToLower() == "excel")
            {
                return GenerateExcel(data, sheetName);
            }
            if (format.ToLower() == "pdf")
            {
                return GeneratePdf(data, sheetName);
            }
            if (format.ToLower() == "csv")
            {
                return GenerateCsv(data);
            }
            throw new NotImplementedException($"Format '{format}' is not supported.");
        }

        /// <summary>
        /// Generates an Excel file using ClosedXML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            if (data.Any())
            {
                worksheet.Cell(1, 1).InsertTable(data);
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Generates a PDF file using iTextSharp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="reportTitle"></param>
        /// <returns></returns>
        private byte[] GeneratePdf<T>(IEnumerable<T> data, string reportTitle)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(0, 0, 255));
            var title = new Paragraph(reportTitle + " Report", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph("\n"));

            // Create table
            var properties = typeof(T).GetProperties();
            var table = new PdfPTable(properties.Length);
            table.WidthPercentage = 100;

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            foreach (var prop in properties)
            {
                var cell = new PdfPCell(new Phrase(prop.Name, headerFont));
                cell.BackgroundColor = new BaseColor(211, 211, 211);
                cell.Padding = 5;
                table.AddCell(cell);
            }

            // Add data rows
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var item in data)
            {
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    var displayValue = value switch
                    {
                        DateTime dt => dt.ToString("MMM dd, yyyy"),
                        null => "",
                        _ => value?.ToString() ?? ""
                    };
                    table.AddCell(new PdfPCell(new Phrase(displayValue, dataFont)) { Padding = 5 });
                }
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Generates a CSV file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] GenerateCsv<T>(IEnumerable<T> data)
        {
            var csvContent = new StringBuilder();
            var properties = typeof(T).GetProperties();

            csvContent.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item)?.ToString() ?? "");
                csvContent.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(csvContent.ToString());
        }

        /// <summary>
        /// Generate User Excel Report with specific columns and formatting
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="filterDetails"></param>
        /// <returns></returns>
        private byte[] GenerateUserExcel(List<UserReportData> userData, string filterDetails)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("User Report");

            // Add title and filter details
            worksheet.Cell(1, 1).Value = "USER REPORT";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            worksheet.Cell(2, 1).Value = "Generated Date: " + DateTime.Now.ToString("MMM dd, yyyy HH:mm");
            worksheet.Cell(3, 1).Value = "Filter Details: " + filterDetails;

            // Add headers
            var headers = new[] { "Full Name", "Username", "Email", "NIC", "Address", "Status", "Created At", "Role" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.Gray;
            }

            // Add data
            int row = 6;
            foreach (var user in userData)
            {
                worksheet.Cell(row, 1).Value = user.FullName;
                worksheet.Cell(row, 2).Value = user.Username;
                worksheet.Cell(row, 3).Value = user.Email;
                worksheet.Cell(row, 4).Value = user.NIC;
                worksheet.Cell(row, 5).Value = user.Address;
                worksheet.Cell(row, 6).Value = user.Status;
                worksheet.Cell(row, 7).Value = user.CreatedAt;
                worksheet.Cell(row, 8).Value = user.Role;
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Generate Event Excel Report with specific columns and formatting
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="filterDetails"></param>
        /// <returns></returns>
        private byte[] GenerateEventExcel(List<EventReportData> eventData, string filterDetails)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Event Report");

            // Add title and filter details
            worksheet.Cell(1, 1).Value = "EVENT REPORT";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            worksheet.Cell(2, 1).Value = "Generated Date: " + DateTime.Now.ToString("MMM dd, yyyy HH:mm");
            worksheet.Cell(3, 1).Value = "Filter Details: " + filterDetails;

            // Add headers
            var headers = new[] { "Event Title", "Category", "Organizer", "Event Date", "Capacity", "Status", "Created Date" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(5, i + 1).Value = headers[i];
                worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.Gray;
            }

            // Add data
            int row = 6;
            foreach (var eventItem in eventData)
            {
                worksheet.Cell(row, 1).Value = eventItem.EventTitle;
                worksheet.Cell(row, 2).Value = eventItem.Category;
                worksheet.Cell(row, 3).Value = eventItem.Organizer;
                worksheet.Cell(row, 4).Value = eventItem.EventDate;
                worksheet.Cell(row, 5).Value = eventItem.Capacity;
                worksheet.Cell(row, 6).Value = eventItem.Status;
                worksheet.Cell(row, 7).Value = eventItem.CreatedDate;
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Generate User PDF Report with specific columns and formatting
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="filterDetails"></param>
        /// <returns></returns>
        private byte[] GenerateUserPdf(List<UserReportData> userData, string filterDetails)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(0, 0, 255));
            var title = new Paragraph("USER REPORT", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph("\n"));

            // Add generation date and filter details
            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            document.Add(new Paragraph($"Generated Date: {DateTime.Now:MMM dd, yyyy HH:mm}", infoFont));
            document.Add(new Paragraph($"Filter Details: {filterDetails}", infoFont));
            document.Add(new Paragraph("\n"));

            // Create table
            var table = new PdfPTable(8); // 8 columns including email
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 2f, 1.5f, 2f, 1.5f, 2f, 1f, 1.5f, 1f });

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var headers = new[] { "Full Name", "Username", "Email", "NIC", "Address", "Status", "Created At", "Role" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, headerFont));
                cell.BackgroundColor = new BaseColor(211, 211, 211);
                cell.Padding = 5;
                table.AddCell(cell);
            }

            // Add data rows
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var user in userData)
            {
                table.AddCell(new PdfPCell(new Phrase(user.FullName, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(user.Username, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(user.Email, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(user.NIC, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(user.Address, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(user.Status, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(user.CreatedAt, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(user.Role, dataFont)) { Padding = 5 });
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Generate Event PDF Report with specific columns and formatting
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="filterDetails"></param>
        /// <returns></returns>
        private byte[] GenerateEventPdf(List<EventReportData> eventData, string filterDetails)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(0, 0, 255));
            var title = new Paragraph("EVENT REPORT", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph("\n"));

            // Add generation date and filter details
            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            document.Add(new Paragraph($"Generated Date: {DateTime.Now:MMM dd, yyyy HH:mm}", infoFont));
            document.Add(new Paragraph($"Filter Details: {filterDetails}", infoFont));
            document.Add(new Paragraph("\n"));

            // Create table
            var table = new PdfPTable(7); // 7 columns
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 2.5f, 1.5f, 2f, 1.5f, 1f, 1.5f, 1.5f });

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var headers = new[] { "Event Title", "Category", "Organizer", "Event Date", "Capacity", "Status", "Created Date" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, headerFont));
                cell.BackgroundColor = new BaseColor(211, 211, 211);
                cell.Padding = 5;
                table.AddCell(cell);
            }

            // Add data rows
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var eventItem in eventData)
            {
                table.AddCell(new PdfPCell(new Phrase(eventItem.EventTitle, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(eventItem.Category, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(eventItem.Organizer, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(eventItem.EventDate, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(eventItem.Capacity.ToString(), dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(eventItem.Status, dataFont)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(eventItem.CreatedDate, dataFont)) { Padding = 5 });
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }
    }

    // Data classes for report generation
    public class UserReportData
    {
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string NIC { get; set; } = "";
        public string Address { get; set; } = "";
        public string Status { get; set; } = "";
        public string CreatedAt { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class EventReportData
    {
        public string EventTitle { get; set; } = "";
        public string Category { get; set; } = "";
        public string Organizer { get; set; } = "";
        public string EventDate { get; set; } = "";
        public int Capacity { get; set; }
        public string Status { get; set; } = "";
        public string CreatedDate { get; set; } = "";
    }
}
