# EasExpo – Stall Rent Management System

EasExpo is an ASP.NET Core MVC application that fulfils the SRS for a stall rent management platform. It supports three roles—Admin, Stall Owner, and Customer—and delivers role-based dashboards, booking workflows, and Razorpay-backed online payments.

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
- Request bookings, pay securely through Razorpay, and inspect booking history.
- Leave feedback after completed reservations.

## Tech Stack
- ASP.NET Core MVC 3.1 (Views in Razor `.cshtml`).
- Entity Framework Core with SQL Server (connection string in `appsettings.json`).
- ASP.NET Core Identity for authentication and role management.
- Bootstrap-styled UI for a simple, responsive look.
- Razorpay checkout integration for collecting stall rent online.

## Getting Started

```bash
# Install dependencies and build
cd EasExpo
DOTNET_NOLOGO=1 dotnet build

# Run the development server
DOTNET_NOLOGO=1 dotnet run
```

- The app expects a SQL Server instance reachable with the connection string configured under `ConnectionStrings:DefaultConnection`.
- Configure Razorpay keys in `appsettings.json` (and `appsettings.Development.json` if required) under the `Razorpay` section: `KeyId` and `KeySecret`.
- The sample connection string uses Windows Integrated Security; run the app from an environment that can authenticate against that SQL Server instance (for containers or non-Windows hosts, switch to SQL authentication or expose the server appropriately).
- Seeded accounts:
  - Admin — `admin@easexpo.com` / `Admin@123`
  - Stall Owner — `owner@easexpo.com` / `Owner@123`
  - Customer — `customer@easexpo.com` / `Customer@123`

> ℹ️ The project targets .NET Core 3.1, which is out of mainstream support. You can still run it with the .NET 8 SDK installed in the dev container, or upgrade the target framework to a supported LTS release as a follow-up improvement.

## SQL Server connectivity checklist

If the application crashes during startup with a `SqlException` stating that the server was not found (provider error 26/53/35), walk through the following steps.

### 1. Ensure SQL Server is running

```powershell
# Windows – list SQL Server related services
Get-Service *sql*

# Start a specific service if needed
Start-Service MSSQL$SQLEXPRESS
Start-Service SQLBrowser

# LocalDB
SqlLocalDB info
SqlLocalDB start "MSSQLLocalDB"
```

### 2. Confirm the connection string

Update `ConnectionStrings:DefaultConnection` in `appsettings.json` / `appsettings.Development.json` to match your environment:

| Setup | Example connection string |
| --- | --- |
| Visual Studio LocalDB | `Server=(localdb)\MSSQLLocalDB;Database=EasExpo;Trusted_Connection=True;MultipleActiveResultSets=true;` |
| Default local SQL Server instance | `Server=localhost;Database=EasExpo;Trusted_Connection=True;MultipleActiveResultSets=true;` |
| Named instance (e.g. SQLEXPRESS) | `Server=localhost\SQLEXPRESS;Database=EasExpo;Trusted_Connection=True;MultipleActiveResultSets=true;` |
| Remote server with SQL authentication | `Server=sql.example.com,1433;Database=EasExpo;User Id=appuser;Password=StrongP@ssw0rd!;Encrypt=True;TrustServerCertificate=True;` |

Remember to escape backslashes (`\\`) inside JSON files.

### 3. Test connectivity manually

```bash
# SQL authentication
sqlcmd -S localhost\SQLEXPRESS -d master -U sa -P StrongP@ssw0rd! -Q "SELECT @@SERVERNAME"

# Windows authentication (run in PowerShell/CMD)
sqlcmd -S localhost\SQLEXPRESS -Q "SELECT @@SERVERNAME"
```

If SSMS is installed, connect with the same server name and credentials. Success there means the connection string is valid.

### 4. Discover available instances

```bash
sqlcmd -L                    # Broadcast search for SQL Servers
```

```powershell
Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL'
SqlLocalDB info
```

Use the discovered instance name in your connection string.

### 5. Retry migrations and run the app

```powershell
dotnet ef database drop --force --context EasExpoDbContext
dotnet ef database update
dotnet run
```

If the database still cannot be reached, the startup log now includes a pointer to this checklist for deeper troubleshooting.

## Folder Highlights
- `Controllers/` – MVC controllers for each role.
- `Models/` – Entity models, enums, constants, and view models.
- `Views/` – Razor pages for account flows, dashboards, stalls, bookings, and shared layout.
- `Data/DbSeeder.cs` – Bootstraps roles, users, demo stalls, and sample transactions.

## Next Steps
- Add email notifications for booking approvals and payment receipts.
- Build automated reporting exports (CSV/PDF) for admins.
- Upgrade to .NET 6/8 for a fully supported runtime.
