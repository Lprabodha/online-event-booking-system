# 🎟️ Online Event Ticket Booking System

ASP.NET Core 9 MVC + Identity project for managing events, ticket sales, and bookings.  
Supports **customers** (browse & buy), **organizers** (publish/manage events), and **admins** (users, reports).

---

## ✨ Features

- **Authentication & Roles**  
  Secure login with ASP.NET Core Identity, supporting `ApplicationUser` and roles:  
  - `Admin` (manage users/roles/reports)  
  - `Organizer` (create/manage events)  
  - `Customer` (browse & book tickets)

- **Event Management**  
  - Create and manage events, sessions, venues, ticket types, and promotions.

- **Ticket Booking**  
  - Customers browse events, add tickets to cart, and checkout.
  - Each booking generates unique QR-coded tickets for entry.

- **Payments**  
  - Integrates external payment providers (Stripe, PayPal, SolidGate, etc).
  - Orders and payments tracked separately from bookings.

- **Booking & Tickets**  
  - Scannable QR tickets for each booking.
  - Gate-side validation supported.

- **Reports**  
  - Organizers and admins can view sales, attendance, and booking statistics.

---

## 🛠 Tech Stack

- **Backend:** ASP.NET Core 9 (MVC)
- **Auth:** ASP.NET Core Identity (`ApplicationUser`, roles, seeding)
- **Database:** EF Core 9 + MySQL (Pomelo provider)
- **Frontend:** Razor Views + Bootstrap 5
- **Migrations:** EF Core Tools
- **QR Codes:** QRCoder library

---

## 📂 Project Structure

```text
/Controllers
  /Public
    EventsController.cs
    CartController.cs
    CheckoutController.cs
  /Customer
    OrdersController.cs
    BookingsController.cs
    TicketsController.cs
  /Organizer
    EventsController.cs
    SessionsController.cs
    TicketTypesController.cs
    VenuesController.cs
    PromotionsController.cs
    ReportsController.cs
  /Admin
    UsersController.cs
    RolesController.cs
    ReportsController.cs

/Data
  /Entities
    ApplicationUser.cs
    Event.cs
    Session.cs
    Venue.cs
    TicketType.cs
    Booking.cs
    Ticket.cs
    Order.cs
    OrderItem.cs
    Payment.cs
    Promotion.cs
    ApplicationDbContext.cs
  /Seeder
    UserRoleSeeder.cs

/Views
  /Shared
  /Events
  /Cart
  /Checkout
  /Orders
  /Bookings
  /Tickets
  /Venues
  /Promotions
  /Reports

/wwwroot
  (static files: css, js, images, qr outputs)
```

---

## 🚀 Getting Started

### 1. Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- MySQL 8.x or MariaDB 10.x
- EF Core tools

  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 2. Clone the repository

```bash
git clone git@github.com:Lprabodha/online-event-booking.git
cd ticket-booking-system
```

### 3. Configure connection string

Update `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TicketBooking;User Id=root;Password=yourpassword;SslMode=None;"
}
```

### 4. Apply migrations & seed roles/users

```bash
dotnet ef database update
```

The seeder will create these test users:
- **Admin:** admin@mail.com / Password@123
- **Organizer:** organizer@mail.com / Password@123
- **Customer:** customer@mail.com / Password@123

### 5. Run the app

```bash
dotnet run
```

Browse: https://localhost:5001

---

## 📖 Usage Flow

**Customer**
- Browse events → add tickets to cart → checkout → payment → get QR-coded tickets.

**Organizer**
- Create/manage events, sessions, ticket types, venues.
- Monitor sales and attendance reports.

**Admin**
- Manage users, roles, and overall reports.

---

## 🧩 Next Steps / Improvements

- Payment gateway integration (Stripe/SolidGate)
- Email/SMS notifications (order confirmation, ticket delivery)
- Seat reservation (seat map + holds)
- Exportable invoices
- API endpoints for mobile apps

---

## 👨‍💻 Development

Common EF Core commands:

```bash
# Add migration
dotnet ef migrations add Init

# Apply migration
dotnet ef database update
```

---

## 📸 Sample Screenshots / GIFs

> _Coming soon!_  
> Add screenshots or GIFs of booking flow, event creation, QR ticket, admin dashboard.

---

## 🏫 Esoft AD Coursework Project

**Team Members:**  
- Amandi  
- Tharindu Nuwan  
- Dulan  
- Lakkhee  
- Lahiru  

---

## 📜 License

MIT License – feel free to use and adapt.
