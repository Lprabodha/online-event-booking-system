# Stripe Payment Integration Setup

This document provides instructions for setting up the Stripe payment integration in the online event booking system.

## Prerequisites

1. Stripe account (create one at https://stripe.com)
2. .NET 8.0 SDK
3. Visual Studio or VS Code

## Configuration Steps

### 1. Stripe Account Setup

1. Log in to your Stripe Dashboard
2. Navigate to Developers > API Keys
3. Copy your Publishable key and Secret key
4. For webhooks, go to Developers > Webhooks and add a new endpoint:
   - URL: `https://yourdomain.com/api/stripewebhook/webhook`
   - Events to send: `payment_intent.succeeded`, `payment_intent.payment_failed`

### 2. Application Configuration

Update your `appsettings.json` file with your Stripe keys:

```json
{
  "StripeSettings": {
    "PublishableKey": "pk_test_your_actual_publishable_key_here",
    "SecretKey": "sk_test_your_actual_secret_key_here",
    "WebhookSecret": "whsec_your_webhook_secret_here"
  }
}
```

### 3. Email Configuration

Update your SMTP settings in `appsettings.json`:

```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true,
    "SenderName": "Event Booking System",
    "SenderEmail": "your-email@gmail.com"
  }
}
```

### 4. Database Setup

Run the following commands to update your database:

```bash
dotnet ef migrations add AddStripeIntegration
dotnet ef database update
```

## Features Implemented

### ✅ Payment Processing
- Stripe payment gateway integration
- Secure card processing with Stripe Elements
- Payment intent creation and confirmation
- Webhook handling for payment status updates

### ✅ Ticket Management
- Dynamic ticket selection based on event pricing
- QR code generation for each ticket
- Ticket validation and stock management
- Booking reference generation

### ✅ Email Notifications
- Automatic email sending after successful booking
- HTML-formatted emails with ticket QR codes
- Booking confirmation details

### ✅ Discount System
- Discount code validation
- Percentage and fixed amount discounts
- Usage limit tracking
- Real-time discount application

### ✅ Loyalty Points
- Automatic points awarding (10 points per ticket)
- Points tracking per customer
- Integration with booking process

### ✅ Validation & Security
- Comprehensive booking validation
- Stock availability checking
- Customer eligibility verification
- Secure payment processing

### ✅ User Experience
- Modern, responsive checkout UI
- Real-time order summary updates
- Loading states and error handling
- Mobile-friendly design

## Testing

### Test Card Numbers (Stripe Test Mode)

- **Successful payment**: 4242 4242 4242 4242
- **Declined payment**: 4000 0000 0000 0002
- **Requires authentication**: 4000 0025 0000 3155

Use any future expiry date and any 3-digit CVC.

### Test Discount Codes

Create test discount codes in your Stripe Dashboard or add them to your database:

```sql
INSERT INTO Discounts (Id, Code, Type, Value, ValidFrom, ValidTo, IsActive, UsageLimit, UsedCount, CreatedAt)
VALUES 
(NEWID(), 'EARLY10', 'Percent', 10, GETDATE(), DATEADD(MONTH, 1, GETDATE()), 1, 100, 0, GETDATE()),
(NEWID(), 'SAVE20', 'Amount', 20, GETDATE(), DATEADD(MONTH, 1, GETDATE()), 1, 50, 0, GETDATE());
```

## Security Considerations

1. **Never commit real API keys** to version control
2. **Use environment variables** for production secrets
3. **Enable HTTPS** in production
4. **Verify webhook signatures** (implemented)
5. **Validate all inputs** on the server side
6. **Use HTTPS endpoints** for webhook URLs

## Troubleshooting

### Common Issues

1. **Payment fails**: Check Stripe keys are correct and in test mode
2. **QR codes not generating**: Ensure QRCoder package is installed
3. **Emails not sending**: Verify SMTP settings and credentials
4. **Webhooks not working**: Check webhook URL is accessible and signature verification

### Logs

Check application logs for detailed error messages:
- Payment processing errors
- Email sending failures
- Database connection issues

## Production Deployment

1. **Use live Stripe keys** (pk_live_... and sk_live_...)
2. **Update webhook URLs** to production domain
3. **Enable HTTPS** for secure communication
4. **Set up monitoring** for payment failures
5. **Configure backup email** providers
6. **Set up database backups**

## Support

For issues related to:
- **Stripe integration**: Check Stripe documentation and dashboard logs
- **Application errors**: Review application logs and database
- **Email delivery**: Verify SMTP settings and email provider limits
