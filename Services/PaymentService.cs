using Stripe;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models;
using Microsoft.Extensions.Options;

namespace online_event_booking_system.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IOptions<StripeSettings> stripeSettings, ILogger<PaymentService> logger)
        {
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        /// <summary>
        /// Create a payment intent with Stripe
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="customerId"></param>
        /// <param name="eventId"></param>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, string eventId, Guid bookingId)
        {
            try
            {
                var service = new PaymentIntentService();
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents (LKR doesn't use decimal places)
                    Currency = currency.ToLower(),
                    Customer = customerId,
                    Metadata = new Dictionary<string, string>
                    {
                        { "eventId", eventId },
                        { "customerId", customerId },
                        { "bookingId", bookingId.ToString() }
                    },
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                };

                var paymentIntent = await service.CreateAsync(options);

                return new PaymentIntent
                {
                    Id = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Status = paymentIntent.Status,
                    Amount = amount,
                    Currency = currency
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent");
                throw new Exception($"Payment creation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Confirm a payment intent with Stripe
        /// </summary>
        /// <param name="paymentIntentId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                return new PaymentIntent
                {
                    Id = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Status = paymentIntent.Status,
                    Amount = (decimal)paymentIntent.Amount / 100,
                    Currency = paymentIntent.Currency.ToUpper()
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error confirming payment intent");
                throw new Exception($"Payment confirmation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Refund a payment intent with Stripe
        /// </summary>
        /// <param name="paymentIntentId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
        {
            try
            {
                var service = new RefundService();
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = amount.HasValue ? (long)(amount.Value * 100) : null,
                };

                await service.CreateAsync(options);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating refund");
                return false;
            }
        }

        // IPaymentService implementation overload without amount
        public async Task<bool> RefundPaymentAsync(string transactionId)
        {
            return await RefundPaymentAsync(transactionId, null);
        }

        /// <summary>
        /// Create or get an existing Stripe customer for the given user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Customer> CreateOrGetStripeCustomerAsync(ApplicationUser user)
        {
            try
            {
                var customerService = new CustomerService();

                // Try to find existing customer by email
                var existingCustomers = await customerService.ListAsync(new CustomerListOptions
                {
                    Email = user.Email
                });

                if (existingCustomers.Data.Any())
                {
                    var existing = existingCustomers.Data.First();
                    return new Customer
                    {
                        Id = existing.Id,
                        Email = existing.Email,
                        Name = existing.Name
                    };
                }

                // Create new customer
                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = user.Email,
                    Name = user.FullName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", user.Id }
                    }
                });

                return new Customer
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    Name = customer.Name
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating customer");
                throw new Exception($"Customer creation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate the status of a payment intent
        /// </summary>
        /// <param name="paymentIntentId"></param>
        /// <returns></returns>
        public async Task<bool> ValidatePaymentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);
                return paymentIntent.Status == "succeeded";
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error validating payment");
                return false;
            }
        }
    }
}
