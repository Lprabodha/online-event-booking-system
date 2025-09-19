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
        /// <summary>
        /// Display the reports dashboard with filtering options.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Fetch users based on filters and return partial view.
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="role"></param>
        /// <returns></returns>

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
        /// <summary>
        /// Fetch events based on filters and return partial view.
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <returns></returns>

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

        /// <summary>
        /// Revenue summary for events within filters (date, category, organizer).
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("RevenueSummary")]
        public async Task<IActionResult> RevenueSummary(DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null)
        {
            try
            {
                var paymentsQuery = _context.Payments
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.Event)
                            .ThenInclude(e => e.Category)
                    .Include(p => p.Tickets)
                        .ThenInclude(t => t.Event)
                            .ThenInclude(e => e.Organizer)
                    .Where(p => p.Status == "Completed");

                if (dateFrom.HasValue)
                {
                    var from = dateFrom.Value.Date;
                    paymentsQuery = paymentsQuery.Where(p => p.PaidAt.Date >= from);
                }
                if (dateTo.HasValue)
                {
                    var to = dateTo.Value.Date;
                    paymentsQuery = paymentsQuery.Where(p => p.PaidAt.Date <= to);
                }
                if (!string.IsNullOrEmpty(category) && category != "All Categories")
                {
                    paymentsQuery = paymentsQuery.Where(p => p.Tickets.Any(t => t.Event.Category != null && t.Event.Category.Name == category));
                }
                if (!string.IsNullOrEmpty(organizer) && organizer != "All Organizers")
                {
                    paymentsQuery = paymentsQuery.Where(p => p.Tickets.Any(t => t.Event.OrganizerId == organizer));
                }

                var payments = await paymentsQuery.ToListAsync();

                var totalRevenue = payments.Sum(p => p.Amount);
                var ticketsSold = payments.SelectMany(p => p.Tickets).Count();
                var orders = payments.SelectMany(p => p.Tickets).Select(t => t.BookingId).Distinct().Count();
                var avgTicketPrice = ticketsSold > 0 ? Math.Round(totalRevenue / ticketsSold, 2) : 0m;

                var daily = payments
                    .GroupBy(p => p.PaidAt.Date)
                    .Select(g => new { date = g.Key, amount = g.Sum(p => p.Amount), tickets = g.SelectMany(p => p.Tickets).Count() })
                    .OrderBy(g => g.date)
                    .ToList();

                return Json(new
                {
                    revenue = totalRevenue,
                    ticketsSold,
                    orders,
                    avgTicketPrice,
                    daily
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to load revenue summary" });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportType"></param>
        /// <param name="format"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <param name="role"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get MIME type based on file format.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
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
