## 🎟️ Online Event Booking System

<p align="center">
  <a href="https://dotnet.microsoft.com/">
    <img alt=".NET 9" src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
  </a>
  <a href="#configuration-environment-variables">
    <img alt="Config" src="https://img.shields.io/badge/Configure-ENV_VARS-0ea5e9?style=for-the-badge&logo=azurekeyvault&logoColor=white">
  </a>
  <a href="https://localhost:5001/">
    <img alt="Live Demo" src="https://img.shields.io/badge/Live_Demo-localhost%3A5001-2563eb?style=for-the-badge&logo=googlechrome&logoColor=white">
  </a>
  <a href="./LICENSE">
    <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-22c55e?style=for-the-badge">
  </a>
  <a href="#tech-stack">
    <img alt="Stripe" src="https://img.shields.io/badge/Payments-Stripe-635BFF?style=for-the-badge&logo=stripe&logoColor=white">
  </a>
  <a href="#features">
    <img alt="AWS S3" src="https://img.shields.io/badge/Storage-AWS_S3-ff9900?style=for-the-badge&logo=amazons3&logoColor=white">
  </a>
</p>

A comprehensive web-based event ticketing platform built for StarEvents Pvt Ltd, enabling seamless event management, ticket booking, and secure payment processing for concerts, theatre shows, and cultural events in Sri Lanka. This project forms part of the AD coursework for the Software Engineering degree at London Metropolitan University.

---

## Table of Contents
- Overview
- Features
- Tech Stack
- Architecture Overview
- Getting Started
- Configuration (Environment Variables)
- Database and Migrations
- Running and Debugging
- Deployment
- Troubleshooting
- Security Notes
- Academic Attribution
- Contributing
- Team Members
- License

---

## Overview
Customers discover and book events. Organizers create events, set pricing, track sales, and validate QR-coded tickets. Admins manage users, roles, and system-wide settings.

---

## Features
- Authentication and roles with ASP.NET Core Identity
  - Roles: Admin, Organizer, Customer
- Event management: events, venues, categories, pricing tiers
- Checkout and payments (Stripe integration included)
- Bookings and QR-coded tickets (generated and stored in S3)
- Organizer dashboard with charts and KPIs
- Email notifications via SMTP

---

## Tech Stack
- ASP.NET Core 9 (MVC + Razor Views)
- Entity Framework Core 9 (Pomelo MySQL provider)
- Identity (ApplicationUser + Roles)
- Chart.js for dashboards
- QRCoder for QR code generation
- AWS S3 for ticket QR storage

---

## Architecture Overview
High-level folders:

```text
Controllers/
  Admin/        Customer/      Organizer/      Public/
Business/
  Interface/    Service/
Data/
  Entities/     ApplicationDbContext.cs
Services/       Helper/
Views/
  Shared/       Customer/      Organizer/      Events/   Checkout/
wwwroot/
  css/  images/  lib/
```

---

## Getting Started

Prerequisites:
- .NET 9 SDK
- MySQL 8.x (or MariaDB 10.x)
- EF Core Tools: `dotnet tool install --global dotnet-ef`

Clone and restore:
```bash
git clone <your-fork-or-repo-url>
cd online-event-booking-system
dotnet restore
```

Set up configuration (see next section), then create the database and run:
```bash
dotnet ef database update
dotnet run
```
Browse: https://localhost:5001 or http://localhost:5000

---

## Configuration (Environment Variables)
Use User Secrets or environment variables for sensitive settings. Do not commit secrets to git.

Required keys (names mirror `appsettings.json`):

```json
ConnectionStrings:DefaultConnection
SmtpSettings:Host
SmtpSettings:Port
SmtpSettings:Username
SmtpSettings:Password
SmtpSettings:EnableSsl
AWS:S3BucketName
AWS:Region
AWS:AccessKey
AWS:SecretKey
StripeSettings:PublishableKey
StripeSettings:SecretKey
StripeSettings:WebhookSecret
```

Using dotnet user-secrets (recommended for local):
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
dotnet user-secrets set "SmtpSettings:Host" "smtp.gmail.com"
dotnet user-secrets set "SmtpSettings:Port" "587"
dotnet user-secrets set "SmtpSettings:Username" "your@gmail.com"
dotnet user-secrets set "SmtpSettings:Password" "app-password"
dotnet user-secrets set "SmtpSettings:EnableSsl" "true"
```

---

## Database and Migrations
Create database and apply migrations:
```bash
dotnet ef database update
```

Add a new migration when changing entities:
```bash
dotnet ef migrations add <Name>
dotnet ef database update
```

---

## Running and Debugging
Development run:
```bash
dotnet run
```

Build and run with watch:
```bash
dotnet watch run
```

Default roles and demo users (if seeding is enabled):
- Admin: admin@mail.com / Password@123
- Organizer: organizer@mail.com / Password@123
- Customer: customer@mail.com / Password@123

---

## Deployment
- Set environment variables in your hosting platform (do not deploy secrets in files)
- Run migrations on startup or via CI/CD: `dotnet ef database update`
- Build: `dotnet publish -c Release -o out`
- Configure reverse proxy/static files as per ASP.NET Core docs

---

## Troubleshooting
- Database connection errors: verify `DefaultConnection` and DB server is running
- 403 on Stripe webhook: confirm `StripeSettings:WebhookSecret` and webhook URL
- Images/QR not loading: ensure AWS bucket CORS and that S3 keys are correct; app uses direct S3 URLs
- 500 errors in production: enable logging, check `Logging:LogLevel` and hosting logs

---

## Security Notes
- Do NOT commit API keys or credentials to source control
- Prefer `dotnet user-secrets` for local dev and environment variables for prod
- Rotate keys regularly; configure least-privilege for AWS IAM

---

## Academic Attribution
This repository is submitted as part of coursework for the Software Engineering degree programme at London Metropolitan University.

- Programme: BSc Software Engineering (or applicable)
- Institution: London Metropolitan University
- Academic Year: 2025

Note: Any opinions, findings, and conclusions in this work are those of the authors and do not necessarily reflect the views of the institution.

---

## Contributing
We welcome contributions! Please follow these steps:

1. Fork the repository and create a branch
   - git checkout -b feat/short-description
2. Make your changes with clear, small commits
   - Use Conventional Commits: feat:, fix:, docs:, refactor:, chore:
3. Ensure the app builds and lints cleanly
   - dotnet build
   - dotnet ef migrations add <Name> (if schema changes)
   - dotnet ef database update
4. Open a Pull Request
   - Describe the change, screenshots for UI, steps to test
   - Reference related issues if applicable

Coding guidelines:
- Favor readability and clear naming
- Avoid breaking changes to public endpoints without discussion
- Do not include secrets in code or config files

---

## Team Members
- Amandi
- Tharindu Nuwan
- Dulan
- Lakkhee
- Lahiru

---

## License
MIT License – free to use and adapt.
