using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using System.Security.Claims;

namespace online_event_booking_system.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public CustomerController(IBookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        [HttpGet("customer")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            var payments = await _bookingService.GetUserPaymentsAsync(userId);
            var recentBookings = bookings.Take(3).ToList();
            var totalSpent = payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            var upcomingEvents = bookings.Where(b => b.Status == "Confirmed" && b.Event.EventDate > DateTime.UtcNow).Count();

            ViewBag.TotalBookings = bookings.Count;
            ViewBag.UpcomingEvents = upcomingEvents;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.RecentBookings = recentBookings;
            ViewBag.RecentPayments = payments.Take(5).ToList();

            return View();
        }

        [HttpGet("customer/events")]
        public async Task<IActionResult> Events(string? search, string? category)
        {
            try
            {
                // Get all published events
                var events = await _context.Events
                    .Include(e => e.Venue)
                    .Include(e => e.Category)
                    .Include(e => e.Organizer)
                    .Include(e => e.Prices)
                    .Where(e => e.IsPublished && e.Status != "Cancelled")
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    events = events.Where(e => 
                        e.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Venue.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Apply category filter
                if (!string.IsNullOrEmpty(category) && Guid.TryParse(category, out var categoryId))
                {
                    events = events.Where(e => e.CategoryId == categoryId).ToList();
                }

                // Get categories for filter dropdown
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Categories = categories;
                ViewBag.SelectedCategory = category;
                ViewBag.SearchTerm = search;

                return View(events);
            }
            catch (Exception ex)
            {
                // Log error if needed
                return View(new List<online_event_booking_system.Data.Entities.Event>());
            }
        }

        [HttpGet("customer/bookings")]
        public async Task<IActionResult> Bookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return View(bookings);
        }

        [HttpGet("customer/profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId);
            return View(user);
        }

        [HttpPost("customer/profile")]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                user.FullName = model.FullName;
                user.ContactNumber = model.ContactNumber;
                user.Address = model.Address;
                user.NIC = model.NIC;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating profile" });
            }
        }

        [HttpGet("customer/loyalty")]
        public async Task<IActionResult> Loyalty()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var loyaltyPoints = await _bookingService.GetUserLoyaltyPointsAsync(userId);
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            var totalSpent = bookings.Where(b => b.Status == "Confirmed").Sum(b => b.Tickets.Sum(t => t.EventPrice?.Price ?? 0));

            ViewBag.LoyaltyPoints = loyaltyPoints;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.TotalBookings = bookings.Count;

            return View();
        }

        [HttpGet("customer/support")]
        public IActionResult Support()
        {
            return View();
        }

        [HttpGet("customer/booking/{id}")]
        public async Task<IActionResult> OrderDetails(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null || booking.CustomerId != userId)
                return NotFound();

            return View(booking);
        }

        [HttpPost("customer/booking/{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Unauthorized" });

            var success = await _bookingService.CancelBookingAsync(id, userId);
            return Json(new { success, message = success ? "Booking cancelled successfully" : "Failed to cancel booking" });
        }
    }
}