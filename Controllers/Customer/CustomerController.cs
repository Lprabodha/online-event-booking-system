using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Services;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace online_event_booking_system.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;
        private readonly ITicketQRService _ticketQRService;
        private readonly IS3Service _s3Service;
        private readonly ILogger<CustomerController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICustomerPdfService _customerPdfService;
        private readonly IEmailService _emailService;

        public CustomerController(
            IBookingService bookingService, 
            ApplicationDbContext context, 
            ITicketQRService ticketQRService,
            IS3Service s3Service,
            ILogger<CustomerController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICustomerPdfService customerPdfService,
            IEmailService emailService)
        {
            _bookingService = bookingService;
            _context = context;
            _ticketQRService = ticketQRService;
            _s3Service = s3Service;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _customerPdfService = customerPdfService;
            _emailService = emailService;
        }

        /// <summary>
        /// Display the customer dashboard with key metrics and recent activity.
        /// </summary>
        /// <returns></returns>
        [HttpGet("customer")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            var payments = await _bookingService.GetUserPaymentsAsync(userId);
            var recentBookings = bookings.Take(3).ToList();

            // Ensure event images use direct URLs for dashboard cards
            foreach (var booking in recentBookings)
            {
                if (booking?.Event != null && !string.IsNullOrEmpty(booking.Event.Image))
                {
                    booking.Event.Image = _s3Service.GetDirectUrl(booking.Event.Image);
                }
            }
            var totalSpent = payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            var upcomingEvents = bookings.Where(b => b.Status == "Confirmed" && b.Event.EventDate > DateTime.UtcNow).Count();

            ViewBag.TotalBookings = bookings.Count;
            ViewBag.UpcomingEvents = upcomingEvents;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.RecentBookings = recentBookings;
            ViewBag.RecentPayments = payments.Take(5).ToList();

            return View();
        }
        /// <summary>
        /// Display a list of available events with search and category filters.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="category"></param>
        /// <returns></returns>

        [HttpGet("customer/events")]
        public async Task<IActionResult> Events(string? search, string? category, string? location)
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
                        (e.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.Venue?.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.Category?.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.Venue?.Location?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                // Apply explicit location filter
                if (!string.IsNullOrWhiteSpace(location))
                {
                    events = events.Where(e =>
                        (e.Venue?.Location?.Contains(location, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.Venue?.Name?.Contains(location, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                // Apply category filter
                if (!string.IsNullOrEmpty(category) && Guid.TryParse(category, out var categoryId))
                {
                    events = events.Where(e => e.CategoryId == categoryId).ToList();
                }

                // Convert event image paths to direct URLs for faster loading
                foreach (var e in events)
                {
                    if (!string.IsNullOrEmpty(e.Image))
                    {
                        e.Image = _s3Service.GetDirectUrl(e.Image);
                    }
                }

                // Get categories for filter dropdown
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Categories = categories;
                ViewBag.SelectedCategory = category;
                ViewBag.SearchTerm = search;
                ViewBag.SelectedLocation = location;

                return View(events);
            }
            catch (Exception ex)
            {
                // Log error if needed
                return View(new List<online_event_booking_system.Data.Entities.Event>());
            }
        }

        /// <summary>
        /// Display the user's booking history with details and actions.
        /// </summary>
        /// <returns></returns>
        [HttpGet("customer/bookings")]
        public async Task<IActionResult> Bookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return View(bookings);
        }
        /// <summary>
        /// Display and edit the user's profile information.
        /// </summary>
        /// <returns></returns>

        [HttpGet("customer/profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(user);
        }

        /// <summary>
        /// Update the user's profile information.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

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
        /// <summary>
        /// Change the user's password.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost("customer/change-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromBody] online_event_booking_system.Models.View_Models.ChangePasswordViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Unauthorized" });

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Invalid input";
                return Json(new { success = false, message = firstError });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault()?.Description ?? "Failed to change password";
                return Json(new { success = false, message = error });
            }

            await _signInManager.RefreshSignInAsync(user);
            return Json(new { success = true, message = "Password changed successfully" });
        }

        /// <summary>
        /// Display the user's loyalty points and rewards summary.
        /// </summary>
        /// <returns></returns>

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

        /// <summary>
        /// Display detailed information about a specific booking, including tickets and QR codes.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("customer/booking/{id}")]
        public async Task<IActionResult> OrderDetails(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            // Use optimized query for better performance
            var booking = await _bookingService.GetBookingByIdOptimizedAsync(id);
            if (booking == null || booking.CustomerId != userId)
                return NotFound();

            // Process event image using direct URL for better performance
            if (!string.IsNullOrEmpty(booking.Event.Image))
            {
                booking.Event.Image = _s3Service.GetDirectUrl(booking.Event.Image);
            }

            // Get QR code URLs for all tickets using direct URLs
            var qrCodeUrls = new Dictionary<Guid, string>();
            foreach (var ticket in booking.Tickets)
            {
                if (!string.IsNullOrEmpty(ticket.QRCode))
                {
                    qrCodeUrls[ticket.Id] = _s3Service.GetDirectUrl(ticket.QRCode);
                }
            }

            ViewBag.QRCodeUrls = qrCodeUrls;
            return View(booking);
        }

        /// <summary>
        /// Download tickets as a PDF document.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("customer/booking/{id}/tickets.pdf")]
        public async Task<IActionResult> DownloadTickets(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null || booking.CustomerId != userId)
                return NotFound();

            var bytes = await _customerPdfService.GenerateTicketsPdfAsync(booking);
            var fileName = $"tickets_{booking.BookingReference}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Download invoice as a PDF document.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("customer/booking/{id}/invoice.pdf")]
        public async Task<IActionResult> DownloadInvoice(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null || booking.CustomerId != userId)
                return NotFound();

            var bytes = await _customerPdfService.GenerateInvoicePdfAsync(booking);
            var fileName = $"invoice_{booking.BookingReference}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Cancel a booking if eligible.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("customer/booking/{id}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Unauthorized" });

            var success = await _bookingService.CancelBookingAsync(id, userId);

            if (!success)
            {
                return Json(new { success = false, message = "Cancellation not allowed (must be 24+ hours before start) or booking not found." });
            }

            // Try refund
            var refunded = await _bookingService.RefundBookingAsync(id);
            if (refunded)
            {
                try
                {
                    var booking = await _bookingService.GetBookingByIdAsync(id);
                    var payment = booking?.Tickets?.FirstOrDefault()?.Payment;
                    if (booking?.Customer?.Email != null && payment != null)
                    {
                        await _emailService.SendRefundEmailAsync(
                            booking.Customer.Email!,
                            booking.Customer.FullName ?? "Customer",
                            booking.Event.Title,
                            booking.Event.EventDate,
                            payment.Amount,
                            booking.BookingReference
                        );
                    }
                }
                catch { }
            }

            return Json(new { success = true, message = refunded ? "Booking cancelled and refund initiated." : "Booking cancelled. Refund unavailable." });
        }
    }
}