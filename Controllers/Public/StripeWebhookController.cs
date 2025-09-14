using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using Stripe;

namespace online_event_booking_system.Controllers.Public
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IConfiguration _configuration;

        public StripeWebhookController(
            IBookingService bookingService,
            ILogger<StripeWebhookController> logger,
            IConfiguration configuration)
        {
            _bookingService = bookingService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _configuration["StripeSettings:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentSucceeded:
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentSucceeded(paymentIntent!);
                        break;

                    case Events.PaymentIntentPaymentFailed:
                        var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentFailed(failedPayment!);
                        break;

                    default:
                        _logger.LogInformation($"Unhandled event type: {stripeEvent.Type}");
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook error");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing error");
                return StatusCode(500);
            }
        }

        private async Task HandlePaymentSucceeded(PaymentIntent paymentIntent)
        {
            try
            {
                // Find booking by payment intent ID
                var bookingId = paymentIntent.Metadata.GetValueOrDefault("bookingId");
                if (!string.IsNullOrEmpty(bookingId) && Guid.TryParse(bookingId, out var bookingGuid))
                {
                    await _bookingService.ProcessPaymentAsync(paymentIntent.Id, bookingGuid);
                    _logger.LogInformation($"Payment succeeded for booking {bookingGuid}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling payment succeeded for {paymentIntent.Id}");
            }
        }

        private async Task HandlePaymentFailed(PaymentIntent paymentIntent)
        {
            try
            {
                _logger.LogWarning($"Payment failed for {paymentIntent.Id}: {paymentIntent.LastPaymentError?.Message}");
                // Handle failed payment logic here if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling payment failed for {paymentIntent.Id}");
            }
        }
    }
}
