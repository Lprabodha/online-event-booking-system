using ClosedXML.Excel;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Models;
using online_event_booking_system.Repository.Interface;
using online_event_booking_system.Data.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace online_event_booking_system.Business.Service
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<byte[]> GenerateReportAsync(string reportType, string format, DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null, string role = null)
        {
            switch (reportType.ToLower())
            {
                case "events":
                    var events = await _reportRepository.GetEventsAsync(dateFrom, dateTo, category, organizer);
                    return GenerateFile(events, format, "Events");
                case "users":
                    var users = await _reportRepository.GetUsersAsync(dateFrom, dateTo, role);
                    return GenerateFile(users, format, "Users");
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

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync(DateTime? dateFrom, DateTime? dateTo, string role = null)
        {
            return await _reportRepository.GetUsersAsync(dateFrom, dateTo, role);
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null)
        {
            return await _reportRepository.GetEventsAsync(dateFrom, dateTo, category, organizer);
        }

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

        // Generates an Excel file using ClosedXML
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

        // Generates a PDF file using QuestPDF
        private byte[] GeneratePdf<T>(IEnumerable<T> data, string reportTitle)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);

                    page.Header()
                        .Text(reportTitle + " Report")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            var properties = typeof(T).GetProperties();
                            
                            table.ColumnsDefinition(columns =>
                            {
                                // Create columns based on the number of properties
                                for (int i = 0; i < properties.Length; i++)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            // Header row
                            foreach (var prop in properties)
                            {
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(prop.Name).SemiBold();
                            }

                            // Data rows
                            foreach (var item in data)
                            {
                                foreach (var prop in properties)
                                {
                                    var value = prop.GetValue(item);
                                    var displayValue = value switch
                                    {
                                        DateTime dt => dt.ToString("MMM dd, yyyy"),
                                        null => "",
                                        _ => value.ToString()
                                    };
                                    table.Cell().Padding(5).Text(displayValue);
                                }
                            }
                        });
                });
            });

            return document.GeneratePdf();
        }

        // Generates a CSV file manually
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
    }
}
