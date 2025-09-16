using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Data;
using Microsoft.EntityFrameworkCore;

namespace online_event_booking_system.Controllers.Admin
{
    [Route("Admin/[controller]")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _context;

        public ReportsController(IReportService reportService, ApplicationDbContext context)
        {
            _reportService = reportService;
            _context = context;
        }

        // GET: Admin/Reports
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            var organizers = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && 
                    _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Organizer")))
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Organizers = organizers;
            return View("~/Views/Admin/Reports.cshtml");
        }

        // GET: Admin/Reports/Users
        [HttpGet]
        [Route("Users")]
        public async Task<IActionResult> Users(DateTime? dateFrom, DateTime? dateTo, string role = null)
        {
            try
            {
                var users = await _reportService.GetUsersAsync(dateFrom, dateTo, role);
                return PartialView("~/Views/Admin/_UsersTable.cshtml", users);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                return PartialView("~/Views/Admin/_UsersTable.cshtml", new List<ApplicationUser>());
            }
        }

        // GET: Admin/Reports/Events
        [HttpGet]
        [Route("Events")]
        public async Task<IActionResult> Events(DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null)
        {
            try
            {
                var events = await _reportService.GetEventsAsync(dateFrom, dateTo, category, organizer);
                return PartialView("~/Views/Admin/_EventsTable.cshtml", events);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                return PartialView("~/Views/Admin/_EventsTable.cshtml", new List<Event>());
            }
        }

        // POST: Admin/Reports/Download
        [HttpPost]
        [Route("Download")]
        public async Task<IActionResult> Download(string reportType, string format, DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null, string role = null)
        {
            if (string.IsNullOrEmpty(reportType) || string.IsNullOrEmpty(format))
            {
                return BadRequest("Report type and format are required.");
            }

            try
            {
                var reportData = await _reportService.GenerateReportAsync(reportType, format, dateFrom, dateTo, category, organizer, role);
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
