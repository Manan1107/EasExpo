# EasExpo – Stall Rent Management System

EasExpo is an ASP.NET Core MVC application that fulfils the SRS for a stall rent management platform. It supports three roles—Admin, Stall Owner, and Customer—and demonstrates role-based dashboards, booking workflows, and mock payment processing.

## Features

### Admin
- Manage users with role assignments and activation state.
- Maintain the global stall catalogue and assign owners.
- Review and approve stall-owner applications.
- View payment reports and key metrics from the analytics dashboard.

### Stall Owner
- Review a personalised dashboard for stalls, bookings, and revenue.
- Create, edit, and remove their own stalls.
- Approve or reject customer booking requests.
- Track customer feedback for service improvements.

### Customer
- Register and authenticate securely.
- Browse stalls with search and status filters.
- Request bookings, submit mock payments, and inspect booking history.
- Leave feedback after completed reservations.

## Tech Stack
- ASP.NET Core MVC 3.1 (Views in Razor `.cshtml`).
- Entity Framework Core with SQL Server (connection string in `appsettings.json`).
- ASP.NET Core Identity for authentication and role management.
- Bootstrap-styled UI for a simple, responsive look.

## Getting Started

```bash
# Install dependencies and build
cd EasExpo
DOTNET_NOLOGO=1 dotnet build

# Run the development server
DOTNET_NOLOGO=1 dotnet run
```

- The app expects a SQL Server instance reachable with the connection string configured under `ConnectionStrings:DefaultConnection`.
- The sample connection string uses Windows Integrated Security; run the app from an environment that can authenticate against that SQL Server instance (for containers or non-Windows hosts, switch to SQL authentication or expose the server appropriately).
- Seeded accounts:
  - Admin — `admin@easexpo.com` / `Admin@123`
  - Stall Owner — `owner@easexpo.com` / `Owner@123`
  - Customer — `customer@easexpo.com` / `Customer@123`

> ℹ️ The project targets .NET Core 3.1, which is out of mainstream support. You can still run it with the .NET 8 SDK installed in the dev container, or upgrade the target framework to a supported LTS release as a follow-up improvement.

## Folder Highlights
- `Controllers/` – MVC controllers for each role.
- `Models/` – Entity models, enums, constants, and view models.
- `Views/` – Razor pages for account flows, dashboards, stalls, bookings, and shared layout.
- `Data/DbSeeder.cs` – Bootstraps roles, users, demo stalls, and sample transactions.

## Next Steps
- Swap the mock payment flow for real Razorpay/PayPal gateway integration.
- Add email notifications for booking approvals and payment receipts.
- Upgrade to .NET 6/8 for a fully supported runtime.
