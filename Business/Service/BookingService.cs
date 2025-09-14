using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;
using online_event_booking_system.Services;

namespace online_event_booking_system.Business.Service
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IQRCodeService _qrCodeService;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            ApplicationDbContext context,
            IPaymentService paymentService,
            IQRCodeService qrCodeService,
            IEmailService emailService,
            ILogger<BookingService> logger)
        {
            _context = context;
            _paymentService = paymentService;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<CheckoutViewModel> GetCheckoutDataAsync(Guid eventId)
        {
            var eventData = await _context.Events
                .Include(e => e.Prices.Where(p => p.IsActive))
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventData == null)
                throw new ArgumentException("Event not found");

            var checkoutData = new CheckoutViewModel
            {
                EventId = eventId,
                Event = eventData,
                TicketItems = eventData.Prices.Select(p => new CheckoutTicketItem
                {
                    EventPriceId = p.Id,
                    Category = p.Category,
                    Description = p.Description ?? "",
                    Price = p.Price,
                    Quantity = 0,
                    MaxQuantity = Math.Min(p.MaxQuantity ?? 10, 10),
                    AvailableStock = p.Stock
                }).ToList()
            };

            return checkoutData;
        }

        public async Task<bool> ValidateBookingAsync(ProcessCheckoutRequest request)
        {
            try
            {
                // Validate event exists and is available
                var eventData = await _context.Events
                    .Include(e => e.Prices)
                    .FirstOrDefaultAsync(e => e.Id == request.EventId);

                if (eventData == null || !eventData.IsPublished || eventData.Status != "Published")
                    return false;

                // Check if event is not sold out
                if (eventData.TicketSalesEnd.HasValue && eventData.TicketSalesEnd < DateTime.UtcNow)
                    return false;

                // Validate ticket quantities and availability
                foreach (var ticket in request.Tickets)
                {
                    var price = eventData.Prices.FirstOrDefault(p => p.Id == ticket.EventPriceId);
                    if (price == null || !price.IsActive || ticket.Quantity <= 0)
                        return false;

                    // Check stock availability
                    var currentBookings = await _context.Tickets
                        .Where(t => t.EventPriceId == ticket.EventPriceId)
                        .CountAsync();

                    if (currentBookings + ticket.Quantity > price.Stock)
                        return false;

                    // Check quantity limits
                    if (price.MaxQuantity.HasValue && ticket.Quantity > price.MaxQuantity)
                        return false;
                }

                // Validate discount code if provided
                if (!string.IsNullOrEmpty(request.DiscountCode))
                {
                    var discount = await _context.Discounts
                        .FirstOrDefaultAsync(d => d.Code == request.DiscountCode && 
                                                 d.IsActive && 
                                                 d.ValidFrom <= DateTime.UtcNow && 
                                                 d.ValidTo >= DateTime.UtcNow);

                    if (discount == null)
                        return false;

                    // Check usage limit
                    if (discount.UsageLimit.HasValue && discount.UsedCount >= discount.UsageLimit)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating booking");
                return false;
            }
        }

        public async Task<CheckoutResponse> ProcessCheckoutAsync(ProcessCheckoutRequest request, string userId)
        {
            try
            {
                // Validate booking
                if (!await ValidateBookingAsync(request))
                {
                    return new CheckoutResponse
                    {
                        Success = false,
                        Message = "Invalid booking request. Please check your selections and try again."
                    };
                }

                // Get user and create Stripe customer
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new CheckoutResponse
                    {
                        Success = false,
                        Message = "User not found."
                    };
                }

                var stripeCustomer = await _paymentService.CreateOrGetStripeCustomerAsync(user);

                // Calculate total amount
                var eventData = await _context.Events
                    .Include(e => e.Prices)
                    .FirstOrDefaultAsync(e => e.Id == request.EventId);

                decimal subtotal = 0;
                foreach (var ticket in request.Tickets)
                {
                    var price = eventData!.Prices.First(p => p.Id == ticket.EventPriceId);
                    subtotal += price.Price * ticket.Quantity;
                }

                // Apply discount if provided
                decimal discountAmount = 0;
                Discount? appliedDiscount = null;
                if (!string.IsNullOrEmpty(request.DiscountCode))
                {
                    appliedDiscount = await _context.Discounts
                        .FirstOrDefaultAsync(d => d.Code == request.DiscountCode);
                    
                    if (appliedDiscount != null)
                    {
                        if (appliedDiscount.Type == "Percent")
                            discountAmount = subtotal * (appliedDiscount.Value / 100);
                        else
                            discountAmount = appliedDiscount.Value;
                    }
                }

                decimal serviceFee = subtotal * 0.08m; // 8% service fee
                decimal processingFee = 2.99m;
                decimal total = subtotal + serviceFee + processingFee - discountAmount;

                // Create payment intent
                var paymentIntent = await _paymentService.CreatePaymentIntentAsync(
                    total, "usd", stripeCustomer.Id, request.EventId.ToString());

                // Create booking record
                var booking = new Booking
                {
                    CustomerId = userId,
                    EventId = request.EventId,
                    Status = "Pending",
                    BookingReference = Guid.NewGuid().ToString("N")[..12].ToUpper()
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Create payment record
                var payment = new Payment
                {
                    CustomerId = userId,
                    Amount = total,
                    PaymentMethod = "Stripe",
                    Status = "Pending",
                    Currency = "USD",
                    TransactionId = paymentIntent.Id,
                    Notes = $"Booking for {eventData.Title}"
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Create ticket records
                foreach (var ticket in request.Tickets)
                {
                    var price = eventData.Prices.First(p => p.Id == ticket.EventPriceId);
                    
                    for (int i = 0; i < ticket.Quantity; i++)
                    {
                        var ticketNumber = $"{booking.BookingReference}-{ticket.EventPriceId.ToString()[..8]}-{i + 1:D3}";
                        var qrCodeData = _qrCodeService.GenerateTicketQRCode(
                            Guid.NewGuid(), request.EventId, userId);

                        var ticketRecord = new Ticket
                        {
                            CustomerId = userId,
                            BookingId = booking.Id,
                            EventId = request.EventId,
                            EventPriceId = ticket.EventPriceId,
                            PaymentId = payment.Id,
                            TicketNumber = ticketNumber,
                            QRCode = qrCodeData,
                            IsPaid = false
                        };

                        _context.Tickets.Add(ticketRecord);
                    }
                }

                // Update discount usage if applied
                if (appliedDiscount != null)
                {
                    appliedDiscount.UsedCount++;
                    _context.Discounts.Update(appliedDiscount);
                }

                await _context.SaveChangesAsync();

                return new CheckoutResponse
                {
                    Success = true,
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    BookingId = booking.Id,
                    TotalAmount = total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout");
                return new CheckoutResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your booking. Please try again."
                };
            }
        }

        public async Task<bool> ProcessPaymentAsync(string paymentIntentId, Guid bookingId)
        {
            try
            {
                var paymentValid = await _paymentService.ValidatePaymentAsync(paymentIntentId);
                if (!paymentValid)
                    return false;

                var booking = await _context.Bookings
                    .Include(b => b.Tickets)
                    .Include(b => b.Event)
                    .Include(b => b.Customer)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                    return false;

                // Update booking status
                booking.Status = "Confirmed";
                booking.UpdatedAt = DateTime.UtcNow;

                // Update payment status
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);
                
                if (payment != null)
                {
                    payment.Status = "Completed";
                    payment.PaidAt = DateTime.UtcNow;
                    payment.UpdatedAt = DateTime.UtcNow;
                    _context.Payments.Update(payment);
                }

                // Update ticket status
                foreach (var ticket in booking.Tickets)
                {
                    ticket.IsPaid = true;
                    _context.Tickets.Update(ticket);
                }

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                // Send confirmation email
                await SendBookingConfirmationEmailAsync(booking);

                // Award loyalty points
                await AwardLoyaltyPointsAsync(booking.CustomerId, booking.Tickets.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return false;
            }
        }

        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Customer)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.EventPrice)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<List<Booking>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.EventPrice)
                .Where(b => b.CustomerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CancelBookingAsync(Guid bookingId, string userId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Tickets)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerId == userId);

                if (booking == null || booking.Status != "Confirmed")
                    return false;

                // Check if cancellation is allowed (e.g., within 24 hours of event)
                var eventData = await _context.Events.FindAsync(booking.EventId);
                if (eventData != null && eventData.EventDate <= DateTime.UtcNow.AddHours(24))
                    return false;

                booking.Status = "Cancelled";
                booking.UpdatedAt = DateTime.UtcNow;

                // Mark tickets as cancelled
                foreach (var ticket in booking.Tickets)
                {
                    ticket.IsUsed = true; // Mark as used to prevent further use
                    ticket.UsedAt = DateTime.UtcNow;
                    _context.Tickets.Update(ticket);
                }

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                return false;
            }
        }

        public async Task<bool> RefundBookingAsync(Guid bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Tickets)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null || booking.Status != "Cancelled")
                    return false;

                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.CustomerId == booking.CustomerId && 
                                             p.TransactionId != null);

                if (payment == null || string.IsNullOrEmpty(payment.TransactionId))
                    return false;

                var refundSuccess = await _paymentService.RefundPaymentAsync(payment.TransactionId);
                if (refundSuccess)
                {
                    payment.Status = "Refunded";
                    payment.UpdatedAt = DateTime.UtcNow;
                    _context.Payments.Update(payment);
                    await _context.SaveChangesAsync();
                }

                return refundSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                return false;
            }
        }

        private async Task SendBookingConfirmationEmailAsync(Booking booking)
        {
            try
            {
                var ticketsHtml = string.Join("<br/>", booking.Tickets.Select(t => 
                    $"<div style='border: 1px solid #ddd; padding: 10px; margin: 10px 0; border-radius: 5px;'>" +
                    $"<h3>Ticket: {t.TicketNumber}</h3>" +
                    $"<p><strong>Category:</strong> {t.EventPrice.Category}</p>" +
                    $"<p><strong>Price:</strong> ${t.EventPrice.Price:F2}</p>" +
                    $"<div style='text-align: center; margin: 10px 0;'>" +
                    $"<img src='{t.QRCode}' alt='QR Code' style='max-width: 200px; height: auto;'/>" +
                    $"</div></div>"));

                var emailBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                            .content {{ background: #f9f9f9; padding: 20px; border-radius: 0 0 10px 10px; }}
                            .ticket {{ background: white; border: 1px solid #ddd; padding: 15px; margin: 10px 0; border-radius: 8px; }}
                            .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎫 Booking Confirmation</h1>
                                <p>Your tickets are confirmed!</p>
                            </div>
                            <div class='content'>
                                <h2>Event Details</h2>
                                <p><strong>Event:</strong> {booking.Event.Title}</p>
                                <p><strong>Date:</strong> {booking.Event.EventDate:MMMM dd, yyyy}</p>
                                <p><strong>Time:</strong> {booking.Event.EventTime:HH:mm}</p>
                                <p><strong>Venue:</strong> {booking.Event.Venue?.Name}</p>
                                <p><strong>Booking Reference:</strong> {booking.BookingReference}</p>
                                
                                <h2>Your Tickets</h2>
                                {ticketsHtml}
                                
                                <div class='footer'>
                                    <p>Please bring a valid ID and show your QR code at the entrance.</p>
                                    <p>If you have any questions, please contact our support team.</p>
                                </div>
                            </div>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(
                    booking.Customer.Email!,
                    $"Booking Confirmation - {booking.Event.Title}",
                    emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending booking confirmation email");
            }
        }

        private async Task AwardLoyaltyPointsAsync(string customerId, int ticketCount)
        {
            try
            {
                var pointsPerTicket = 10; // Award 10 points per ticket
                var totalPoints = ticketCount * pointsPerTicket;

                var loyaltyPoint = await _context.LoyaltyPoints
                    .FirstOrDefaultAsync(lp => lp.CustomerId == customerId);

                if (loyaltyPoint != null)
                {
                    loyaltyPoint.Points += totalPoints;
                    loyaltyPoint.LastUpdated = DateTime.UtcNow;
                    loyaltyPoint.Description = $"Awarded {totalPoints} points for purchasing {ticketCount} ticket(s)";
                    _context.LoyaltyPoints.Update(loyaltyPoint);
                }
                else
                {
                    loyaltyPoint = new LoyaltyPoint
                    {
                        CustomerId = customerId,
                        Points = totalPoints,
                        LastUpdated = DateTime.UtcNow,
                        Description = $"Initial points for purchasing {ticketCount} ticket(s)"
                    };
                    _context.LoyaltyPoints.Add(loyaltyPoint);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding loyalty points");
            }
        }
    }
}
