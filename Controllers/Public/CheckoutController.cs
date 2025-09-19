using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Models.View_Models;
using online_event_booking_system.Models;
using online_event_booking_system.Services;

namespace online_event_booking_system.Controllers.Public
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckoutController> _logger;
        private readonly UserManager<online_event_booking_system.Data.Entities.ApplicationUser> _userManager;
        private readonly ITicketQRService _ticketQRService;
        private readonly IS3Service _s3Service;

        public CheckoutController(
            IBookingService bookingService,
            ApplicationDbContext context,
            ILogger<CheckoutController> logger,
            UserManager<online_event_booking_system.Data.Entities.ApplicationUser> userManager,
            ITicketQRService ticketQRService,
            IS3Service s3Service)
        {
            _bookingService = bookingService;
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _ticketQRService = ticketQRService;
            _s3Service = s3Service;
        }

        [HttpGet("checkout/{eventId}")]
        public async Task<IActionResult> Index(Guid eventId)
        {
            try
            {
                var checkoutData = await _bookingService.GetCheckoutDataAsync(eventId);
                
                // Process event image using direct URL for better performance
                if (!string.IsNullOrEmpty(checkoutData.Event.Image))
                {
                    checkoutData.Event.Image = _s3Service.GetDirectUrl(checkoutData.Event.Image);
                }
                
                // Add Stripe publishable key from configuration
                checkoutData.StripePublishableKey = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetValue<string>("StripeSettings:PublishableKey");

                return View(checkoutData);
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["Error"] = "An error occurred while loading the checkout page.";
                return RedirectToAction("Index", "Events");
            }
        }

        [HttpPost("checkout/process")]
        public async Task<IActionResult> ProcessCheckout([FromBody] ProcessCheckoutRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new CheckoutResponse
                    {
                        Success = false,
                        Message = "User not authenticated."
                    });
                }

                var result = await _bookingService.ProcessCheckoutAsync(request, userId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                return Json(new CheckoutResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your booking."
                });
            }
        }

        [HttpPost("checkout/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var success = await _bookingService.ProcessPaymentAsync(request.PaymentIntentId, request.BookingId);
                
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = "Payment confirmed successfully!",
                        redirectUrl = Url.Action("Confirm", "Checkout", new { id = request.BookingId })
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Payment confirmation failed. Please try again." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                return Json(new { 
                    success = false, 
                    message = "An error occurred while confirming your payment." 
                });
            }
        }

        [HttpGet("checkout/confirm/{bookingId}")]
        public async Task<IActionResult> Confirm(Guid bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("Index", "Customer");
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking confirmation");
                TempData["Error"] = "An error occurred while loading your booking.";
                return RedirectToAction("Index", "Customer");
            }
        }

        [HttpPost("checkout/cancel/{bookingId}")]
        public async Task<IActionResult> CancelBooking(Guid bookingId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var success = await _bookingService.CancelBookingAsync(bookingId, userId);
                
                return Json(new { 
                    success = success, 
                    message = success ? "Booking cancelled successfully." : "Failed to cancel booking." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                return Json(new { 
                    success = false, 
                    message = "An error occurred while cancelling your booking." 
                });
            }
        }

        [HttpPost("checkout/validate-discount")]
        public async Task<IActionResult> ValidateDiscount([FromBody] ValidateDiscountRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.DiscountCode))
                {
                    return Json(new { valid = false, message = "Please enter a discount code." });
                }

                var code = request.DiscountCode.Trim();
                var now = DateTime.UtcNow;

                var discount = await _context.Discounts
                    .FirstOrDefaultAsync(d => d.Code == code);

                if (discount == null)
                {
                    return Json(new { valid = false, message = "Invalid coupon code." });
                }

                if (!discount.IsActive)
                {
                    return Json(new { valid = false, message = "This coupon is inactive." });
                }

                if (discount.ValidFrom > now || discount.ValidTo < now)
                {
                    return Json(new { valid = false, message = "This coupon is expired." });
                }

                if (discount.UsageLimit.HasValue && discount.UsedCount >= discount.UsageLimit.Value)
                {
                    return Json(new { valid = false, message = "This coupon has reached its usage limit." });
                }

                if (discount.EventId.HasValue && discount.EventId.Value != request.EventId)
                {
                    return Json(new { valid = false, message = "This coupon is not valid for the selected event." });
                }

                return Json(new {
                    valid = true,
                    message = "Discount code applied.",
                    type = discount.Type,
                    value = discount.Value
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating discount code");
                return Json(new { valid = false, message = "Failed to validate discount code." });
            }
        }
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public Guid BookingId { get; set; }
    }

    public class ValidateDiscountRequest
    {
        public string DiscountCode { get; set; } = string.Empty;
        public Guid EventId { get; set; }
    }
}
