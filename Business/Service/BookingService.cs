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
        private readonly ITicketQRService _ticketQRService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            ApplicationDbContext context,
            IPaymentService paymentService,
            IQRCodeService qrCodeService,
            IEmailService emailService,
            ITicketQRService ticketQRService,
            ILogger<BookingService> logger)
        {
            _context = context;
            _paymentService = paymentService;
            _qrCodeService = qrCodeService;
            _emailService = emailService;
            _ticketQRService = ticketQRService;
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

                // New pricing policy: customer pays only ticket price minus discounts
                decimal total = subtotal - discountAmount;

                // Create booking record first
                var booking = new Booking
                {
                    CustomerId = userId,
                    EventId = request.EventId,
                    Status = "Pending",
                    BookingReference = Guid.NewGuid().ToString("N")[..12].ToUpper()
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Create payment intent with booking ID
                var paymentIntent = await _paymentService.CreatePaymentIntentAsync(
                    total, "lkr", stripeCustomer.Id, request.EventId.ToString(), booking.Id);

                // Create payment record
                var payment = new Payment
                {
                    CustomerId = userId,
                    Amount = total,
                    PaymentMethod = "Stripe",
                    Status = "Pending",
                    Currency = "LKR",
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
                        var ticketId = Guid.NewGuid();
                        var ticketNumber = $"{booking.BookingReference}-{ticket.EventPriceId.ToString()[..8]}-{i + 1:D3}";

                        var ticketRecord = new Ticket
                        {
                            Id = ticketId,
                            CustomerId = userId,
                            BookingId = booking.Id,
                            EventId = request.EventId,
                            EventPriceId = ticket.EventPriceId,
                            PaymentId = payment.Id,
                            TicketNumber = ticketNumber,
                            QRCode = "", // Will be set after payment when QR code is generated and uploaded to S3
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

                // Update ticket status and generate individual QR codes
                foreach (var ticket in booking.Tickets)
                {
                    ticket.IsPaid = true;
                    
                    // Generate QR code and upload to S3, then send individual ticket email
                    try
                    {
                        var qrCodePath = await _ticketQRService.GenerateAndUploadTicketQRCodeAsync(
                            ticketId: ticket.Id,
                            eventId: booking.EventId,
                            customerId: booking.CustomerId,
                            ticketNumber: ticket.TicketNumber,
                            customerEmail: booking.Customer.Email!,
                            customerName: booking.Customer.FullName,
                            eventName: booking.Event.Title,
                            eventDate: booking.Event.EventDate,
                            venueName: booking.Event.Venue?.Name ?? "TBA"
                        );
                        
                        // Store QR code S3 path in ticket
                        ticket.QRCode = qrCodePath;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating QR code for ticket {TicketId}", ticket.Id);
                        // Continue with other tickets even if one fails
                    }
                    
                    _context.Tickets.Update(ticket);
                }

                // Add loyalty points (1 point per LKR 10 spent)
                var pointsEarned = (int)(payment.Amount / 10);
                if (pointsEarned > 0)
                {
                    await AddLoyaltyPointsAsync(booking.CustomerId, pointsEarned, $"Points earned from booking {booking.BookingReference}");
                }

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                // Send comprehensive booking email with all tickets
                await SendComprehensiveBookingEmailAsync(booking);

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
                    .ThenInclude(e => e.Venue)
                .Include(b => b.Customer)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.EventPrice)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<Booking?> GetBookingByIdOptimizedAsync(Guid bookingId)
        {
            // Optimized query that only loads the fields needed by the OrderDetails view
            return await _context.Bookings
                .Where(b => b.Id == bookingId)
                .Select(b => new Booking
                {
                    Id = b.Id,
                    BookingReference = b.BookingReference,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    CustomerId = b.CustomerId, // Include for ownership verification
                    Event = new Event
                    {
                        Id = b.Event.Id,
                        Title = b.Event.Title,
                        Description = b.Event.Description,
                        EventDate = b.Event.EventDate,
                        EventTime = b.Event.EventTime,
                        Image = b.Event.Image,
                        AgeRestriction = b.Event.AgeRestriction,
                        Venue = b.Event.Venue != null ? new Venue
                        {
                            Id = b.Event.Venue.Id,
                            Name = b.Event.Venue.Name,
                            Location = b.Event.Venue.Location,
                            Capacity = b.Event.Venue.Capacity
                        } : null,
                        Category = b.Event.Category != null ? new Category
                        {
                            Id = b.Event.Category.Id,
                            Name = b.Event.Category.Name
                        } : null,
                        Organizer = b.Event.Organizer != null ? new ApplicationUser
                        {
                            Id = b.Event.Organizer.Id,
                            FullName = b.Event.Organizer.FullName
                        } : null
                    },
                    Tickets = b.Tickets.Select(t => new Ticket
                    {
                        Id = t.Id,
                        TicketNumber = t.TicketNumber,
                        QRCode = t.QRCode,
                        EventPrice = t.EventPrice != null ? new EventPrice
                        {
                            Id = t.EventPrice.Id,
                            Price = t.EventPrice.Price,
                            Category = t.EventPrice.Category,
                            Description = t.EventPrice.Description
                        } : null
                    }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }


        public async Task<List<Booking>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.Venue)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.EventPrice)
                .Where(b => b.CustomerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking() // Improve performance by not tracking entities
                .ToListAsync();
        }

        public async Task<List<Booking>> GetUserBookingsOptimizedAsync(string userId)
        {
            // Optimized query that only loads the fields needed by the view
            return await _context.Bookings
                .Where(b => b.CustomerId == userId)
                .Select(b => new Booking
                {
                    Id = b.Id,
                    BookingReference = b.BookingReference,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    Event = new Event
                    {
                        Id = b.Event.Id,
                        Title = b.Event.Title,
                        EventDate = b.Event.EventDate,
                        Venue = b.Event.Venue != null ? new Venue
                        {
                            Id = b.Event.Venue.Id,
                            Name = b.Event.Venue.Name
                        } : null
                    },
                    Tickets = b.Tickets.Select(t => new Ticket
                    {
                        Id = t.Id,
                        EventPrice = t.EventPrice != null ? new EventPrice
                        {
                            Id = t.EventPrice.Id,
                            Price = t.EventPrice.Price
                        } : null
                    }).ToList()
                })
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Payment>> GetUserPaymentsAsync(string userId)
        {
            return await _context.Payments
                .Where(p => p.CustomerId == userId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<LoyaltyPoint?> GetUserLoyaltyPointsAsync(string userId)
        {
            return await _context.LoyaltyPoints
                .FirstOrDefaultAsync(lp => lp.CustomerId == userId);
        }

        public async Task<bool> AddLoyaltyPointsAsync(string userId, int points, string description)
        {
            try
            {
                var loyaltyPoint = await _context.LoyaltyPoints
                    .FirstOrDefaultAsync(lp => lp.CustomerId == userId);

                if (loyaltyPoint == null)
                {
                    loyaltyPoint = new LoyaltyPoint
                    {
                        CustomerId = userId,
                        Points = points,
                        Description = description,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.LoyaltyPoints.Add(loyaltyPoint);
                }
                else
                {
                    loyaltyPoint.Points += points;
                    loyaltyPoint.Description = description;
                    loyaltyPoint.LastUpdated = DateTime.UtcNow;
                    _context.LoyaltyPoints.Update(loyaltyPoint);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loyalty points");
                return false;
            }
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

        private async Task SendComprehensiveBookingEmailAsync(Booking booking)
        {
            try
            {
                // Get QR code URLs for all tickets
                var ticketInfos = new List<online_event_booking_system.Helper.TicketInfo>();
                
                foreach (var ticket in booking.Tickets)
                {
                    try
                    {
                        var qrCodeUrl = await _ticketQRService.GetQRCodeUrlAsync(ticket.QRCode);
                        ticketInfos.Add(new online_event_booking_system.Helper.TicketInfo
                        {
                            TicketNumber = ticket.TicketNumber,
                            Category = ticket.EventPrice?.Category ?? "General Admission",
                            Description = ticket.EventPrice?.Description ?? "Standard ticket",
                            Price = ticket.EventPrice?.Price ?? 0,
                            QRCodeUrl = qrCodeUrl
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting QR code URL for ticket {TicketId}", ticket.Id);
                        // Add ticket without QR code URL
                        ticketInfos.Add(new online_event_booking_system.Helper.TicketInfo
                        {
                            TicketNumber = ticket.TicketNumber,
                            Category = ticket.EventPrice?.Category ?? "General Admission",
                            Description = ticket.EventPrice?.Description ?? "Standard ticket",
                            Price = ticket.EventPrice?.Price ?? 0,
                            QRCodeUrl = ""
                        });
                    }
                }

                // Create comprehensive email with all tickets
                var emailBody = online_event_booking_system.Helper.EmailTemplates.GetMultiTicketBookingTemplate(
                    customerName: booking.Customer.FullName,
                    eventName: booking.Event.Title,
                    eventDate: booking.Event.EventDate,
                    venueName: booking.Event.Venue?.Name ?? "TBA",
                    bookingReference: booking.BookingReference,
                    tickets: ticketInfos
                );

                await _emailService.SendEmailAsync(
                    booking.Customer.Email!,
                    $"Your Tickets for {booking.Event.Title} - Star Events",
                    emailBody);

                _logger.LogInformation("Comprehensive booking email sent successfully for booking {BookingId}", booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending comprehensive booking email for booking {BookingId}", booking.Id);
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
