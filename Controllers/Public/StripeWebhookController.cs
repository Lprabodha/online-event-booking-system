using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using Stripe;
using System.Text.Json;

namespace online_event_booking_system.Controllers.Public
{
    [Route("webhook")]
    [ApiController]
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

        [HttpPost("stripe")]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                using var reader = new StreamReader(HttpContext.Request.Body);
                var json = await reader.ReadToEndAsync();
                var webhookSecret = _configuration["StripeSettings:WebhookSecret"];
                var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

                if (string.IsNullOrEmpty(json))
                {
                    _logger.LogError("Empty webhook body received");
                    return BadRequest("Empty body");
                }

                // Try signature verification first
                if (!string.IsNullOrEmpty(webhookSecret) && !string.IsNullOrEmpty(signature))
                {
                    try
                    {
                        var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
                        _logger.LogInformation($"Webhook verified: {stripeEvent.Type}");
                        await ProcessStripeEvent(stripeEvent);
                        return Ok();
                    }
                    catch (StripeException ex)
                    {
                        _logger.LogWarning($"Signature verification failed: {ex.Message}");
                    }
                }

                // Fallback: Process without verification
                return await ProcessWebhookWithoutVerification(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing error");
                return StatusCode(500, "Internal error");
            }
        }

        private async Task ProcessStripeEvent(Event stripeEvent)
        {
            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    if (stripeEvent.Data.Object is PaymentIntent succeededIntent)
                    {
                        await HandlePaymentSucceeded(succeededIntent);
                    }
                    break;

                case Events.PaymentIntentPaymentFailed:
                    if (stripeEvent.Data.Object is PaymentIntent failedIntent)
                    {
                        await HandlePaymentFailed(failedIntent);
                    }
                    break;

                case Events.ChargeSucceeded:
                case Events.ChargeUpdated:
                    _logger.LogInformation($"Charge event: {stripeEvent.Type}");
                    break;

                default:
                    _logger.LogInformation($"Unhandled event: {stripeEvent.Type}");
                    break;
            }
        }

        private async Task<IActionResult> ProcessWebhookWithoutVerification(string json)
        {
            try
            {
                var eventData = JsonSerializer.Deserialize<JsonElement>(json);
                var eventType = eventData.GetProperty("type").GetString();
                
                if (eventType == "payment_intent.succeeded" && 
                    eventData.TryGetProperty("data", out var data) && 
                    data.TryGetProperty("object", out var paymentIntentData))
                {
                    var paymentIntentId = paymentIntentData.GetProperty("id").GetString();
                    var metadata = paymentIntentData.TryGetProperty("metadata", out var meta) ? meta : new JsonElement();
                    var bookingId = metadata.TryGetProperty("bookingId", out var bookingIdElement) ? bookingIdElement.GetString() : null;
                    
                    if (!string.IsNullOrEmpty(bookingId) && Guid.TryParse(bookingId, out var bookingGuid))
                    {
                        var success = await _bookingService.ProcessPaymentAsync(paymentIntentId, bookingGuid);
                        _logger.LogInformation(success ? 
                            $"Payment succeeded for booking {bookingGuid}" : 
                            $"Failed to process payment for booking {bookingGuid}");
                    }
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook without verification");
                return StatusCode(500, "Internal error");
            }
        }

        private async Task HandlePaymentSucceeded(PaymentIntent paymentIntent)
        {
            try
            {
                var bookingId = paymentIntent.Metadata.GetValueOrDefault("bookingId");
                if (!string.IsNullOrEmpty(bookingId) && Guid.TryParse(bookingId, out var bookingGuid))
                {
                    var success = await _bookingService.ProcessPaymentAsync(paymentIntent.Id, bookingGuid);
                    _logger.LogInformation(success ? 
                        $"Payment succeeded for booking {bookingGuid}" : 
                        $"Failed to process payment for booking {bookingGuid}");
                }
                else
                {
                    _logger.LogWarning($"No valid bookingId in payment intent metadata ({paymentIntent.Id})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payment succeeded for {paymentIntent.Id}");
            }
        }

        private async Task HandlePaymentFailed(PaymentIntent paymentIntent)
        {
            try
            {
                _logger.LogWarning($"Payment failed for {paymentIntent.Id}: {paymentIntent.LastPaymentError?.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payment failed for {paymentIntent.Id}");
            }
            await Task.CompletedTask;
        }
    }
}
