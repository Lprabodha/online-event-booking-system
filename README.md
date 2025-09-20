## 🎟️ Online Event Booking System

<p align="center">
  <a href="https://dotnet.microsoft.com/">
    <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  </a>
  <a href="#configuration-environment-variables">
    <img alt="Config" src="https://img.shields.io/badge/Configure-ENV_VARS-0ea5e9?style=for-the-badge&logo=azurekeyvault&logoColor=white">
  </a>
  <a href="https://localhost:5001/">
    <img alt="Localhost" src="https://img.shields.io/badge/Localhost-5001-2563eb?style=for-the-badge&logo=googlechrome&logoColor=white">
  </a>
  <a href="#tech-stack">
    <img alt="Stripe" src="https://img.shields.io/badge/Payments-Stripe-635BFF?style=for-the-badge&logo=stripe&logoColor=white">
  </a>
  <a href="#features">
    <img alt="AWS S3" src="https://img.shields.io/badge/Storage-AWS_S3-ff9900?style=for-the-badge&logo=amazons3&logoColor=white">
  </a>
</p>

A production‑ready web platform for discovering events, purchasing tickets, and managing sales. Customers buy QR‑coded tickets, organizers create and track events, and admins manage users and reporting.

---

## Table of Contents
- Overview
- Features
- Tech Stack
- Architecture
- Getting Started
- Configuration (Environment Variables)
- Database & Migrations
- Running & Debugging
- Key Workflows
- Deployment
- Troubleshooting
- Security Notes
- Academic Attribution
- Contributing
- Team
- License

---

## Overview
The system supports three roles with tailored experiences: `Customer`, `Organizer`, and `Admin`. Core flows include event publishing, multi‑tier ticket pricing, secure checkout with discount codes and loyalty points, automated emails, and analytics/reporting.

---

## Features
- Authentication & Roles (ASP.NET Core Identity)
  - Roles: Admin, Organizer, Customer
- Event Management
  - Venues, categories, images, multi‑tier ticket pricing, stock control
  - Robust creation flow supporting multiple price tiers
- Checkout & Payments
  - Stripe payment intents and confirmation
  - Discount codes with full validation (active, date range, usage limits, event match)
  - Loyalty points redemption (1 point = LKR 1), applied after discount
  - Accurate totals and modern UI with live updates
- Tickets & Invoices
  - QR‑coded tickets stored on AWS S3
  - PDF invoices showing subtotal, discount, and total
  - Emails include clear order summaries with discount display
- Emails
  - Customer welcome email on registration
  - Organizer invitation email (modern design)
  - Organizer promo emails to selected customers
  - Organizer daily summary email
  - Admin weekly report email (manual trigger, cron‑friendly)
- Reporting
  - Organizer sales report download (`excel`/`pdf`/`csv`) with date range
  - Admin/Organizer dashboards and KPIs

---

## Tech Stack
- ASP.NET Core 8 (MVC + Razor Views)
- Entity Framework Core 8 (Pomelo MySQL provider)
- ASP.NET Core Identity (custom `ApplicationUser`)
- Stripe (payments)
- MailKit/MimeKit (SMTP emails)
- QRCoder (QR generation) + AWS S3 (storage)
- iTextSharp / ClosedXML (PDF/Excel reports)

---

## Architecture
High‑level folders:

```text
Controllers/
  Admin/  Customer/  Organizer/  Public/
Business/
  Interface/  Service/
Data/
  Entities/  ApplicationDbContext.cs
Services/  Helper/
Views/
  Shared/  Customer/  Organizer/  Events/  Checkout/
wwwroot/
  css/  images/  lib/
```

---

## Getting Started

Prerequisites:
- .NET 8 SDK
- MySQL 8.x (or MariaDB 10.x)
- EF Core Tools: `dotnet tool install --global dotnet-ef`

Clone and restore:
```bash
git clone git@github.com:Lprabodha/online-event-booking-system.git
cd online-event-booking-system
dotnet restore
```

Create the database and run:
```bash
dotnet ef database update
dotnet run
```
Browse: `https://localhost:5001` or `http://localhost:5000`

---

## Configuration (Environment Variables)
Use User Secrets or environment variables for all secrets. Do not commit secrets to git.

Keys (mirror `appsettings.json`):
```text
ConnectionStrings:DefaultConnection

SmtpSettings:Server
SmtpSettings:Port
SmtpSettings:Username
SmtpSettings:Password
SmtpSettings:SenderEmail
SmtpSettings:SenderName

AWS:S3BucketName
AWS:Region
AWS:AccessKey
AWS:SecretKey

StripeSettings:PublishableKey
StripeSettings:SecretKey
StripeSettings:WebhookSecret
```

Example (User Secrets):
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=eventbookingdb;Uid=root;Pwd=yourpassword;"
dotnet user-secrets set "StripeSettings:PublishableKey" "pk_test_..."
dotnet user-secrets set "StripeSettings:SecretKey" "sk_test_..."
dotnet user-secrets set "StripeSettings:WebhookSecret" "whsec_..."
dotnet user-secrets set "AWS:S3BucketName" "your-bucket"
dotnet user-secrets set "AWS:Region" "us-east-1"
dotnet user-secrets set "AWS:AccessKey" "AKIA..."
dotnet user-secrets set "AWS:SecretKey" "..."
dotnet user-secrets set "SmtpSettings:Server" "smtp.gmail.com"
dotnet user-secrets set "SmtpSettings:Port" "587"
dotnet user-secrets set "SmtpSettings:Username" "your@gmail.com"
dotnet user-secrets set "SmtpSettings:Password" "app-password"
dotnet user-secrets set "SmtpSettings:SenderEmail" "noreply@yourdomain.com"
dotnet user-secrets set "SmtpSettings:SenderName" "Event Booking System"
```

Stripe webhook (local):
```bash
# In Stripe CLI
stripe listen --forward-to https://localhost:5001/stripe/webhook
```

---

## Database & Migrations
Apply existing migrations:
```bash
dotnet ef database update
```

Add a new migration:
```bash
dotnet ef migrations add <Name>
dotnet ef database update
```

---

## Running & Debugging
Development:
```bash
dotnet run
```

Watch mode:
```bash
dotnet watch run
```

Seed users/roles may be enabled (see `Data/Seeders`). Example credentials:
- Admin: `admin@mail.com` / `Password@123`
- Organizer: `organizer@mail.com` / `Password@123`
- Customer: `customer@mail.com` / `Password@123`

---

## Key Workflows
- Discounts
  - Validate via `POST /checkout/validate-discount` with `discountCode` and `eventId`
  - Handles active flag, start/end dates, usage limit, event applicability
- Checkout
  - Client calculates live totals (subtotal − discount − redeemed points)
  - Server persists `Payment.Amount` and `Payment.DiscountAmount`
- Loyalty Points
  - Redeem on checkout (1 point = LKR 1), capped at order value after discount
  - Available points shown to the customer
- Emails
  - Welcome email on registration
  - Organizer promo emails (compose and send to selected customers)
  - Organizer daily summary email
  - Admin weekly report (manual button, cron/scheduler friendly)
- Reporting
  - Organizer sales report download: `POST /organizer/reports/download-sales` with `format=excel|pdf|csv` and optional `dateFrom/dateTo`

---

## Deployment
- Configure environment variables in your host (no secrets in files)
- Run migrations: `dotnet ef database update`
- Publish: `dotnet publish -c Release -o out`
- Ensure reverse proxy/HTTPS/static files per ASP.NET Core guidance

---

## Troubleshooting
- Database: verify `DefaultConnection` and DB server running
- Stripe webhook 403: check `StripeSettings:WebhookSecret` and webhook URL
- S3 assets/QR not visible: verify bucket CORS and credentials; app uses direct S3 URLs
- Emails not sending: confirm SMTP server/port/credentials and less‑secure/app passwords

---

## Security Notes
- Never commit API keys or credentials
- Use `dotnet user-secrets` locally and environment variables in prod
- Rotate keys regularly; apply least‑privilege IAM policies for AWS

---

## Academic Attribution
This repository is submitted as part of coursework for the Software Engineering degree programme at London Metropolitan University (Academic Year 2025).

---

## Contributing
We welcome contributions:
1) Fork and branch
2) Make focused changes (Conventional Commits: `feat:`, `fix:`, `docs:` …)
3) Ensure build/migrations pass: `dotnet build`, `dotnet ef database update`
4) Open a PR with a clear description and test steps

Guidelines:
- Prefer readability and clear naming
- Avoid breaking public endpoints without prior discussion
- Do not include secrets in code or configs

---

## Team
- Amandi
- Tharindu Nuwan
- Dulan
- Lakkhee
- Lahiru

---

## License
MIT License – free to use and adapt.
