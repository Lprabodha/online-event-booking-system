using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;

namespace online_event_booking_system.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GenerateReport(
            [FromQuery] string reportType,
            [FromQuery] string format,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string category,
            [FromQuery] string status)
        {
            if (string.IsNullOrEmpty(reportType) || string.IsNullOrEmpty(format))
            {
                return BadRequest("Report type and format are required.");
            }

            try
            {
                var reportData = await _reportService.GenerateReportAsync(reportType, format, dateFrom, dateTo, category, status);
                var mimeType = GetMimeType(format);
                var fileName = $"{reportType}_report_{DateTime.Now:yyyyMMddHHmmss}.{format}";

                return File(reportData, mimeType, fileName);
            }
            catch (NotImplementedException)
            {
                return NotFound($"Report type '{reportType}' or format '{format}' is not supported yet.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentReports(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo)
        {
            try
            {
                var recentReports = await _reportService.GetRecentReportsAsync(dateFrom, dateTo);
                return Ok(recentReports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private string GetMimeType(string format)
        {
            return format.ToLower() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                _ => "application/octet-stream",
            };
        }
    }
}
