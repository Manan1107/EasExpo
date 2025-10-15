# EasExpo - Event & Exhibition Management System

[![.NET Core](https://img.shields.io/badge/.NET%20Core-3.1-blue.svg)](https://dotnet.microsoft.com/download)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-green.svg)](https://docs.microsoft.com/en-us/aspnet/core/mvc/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-orange.svg)](https://docs.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-red.svg)](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

EasExpo is a comprehensive web application for managing events and exhibitions, built with ASP.NET Core MVC. It provides a complete platform for event organizers, stall owners, and attendees to manage bookings, payments, and feedback efficiently.

## 📋 Features

### 🎯 Core Functionality
- **Event Management**: Create, update, and manage events with detailed information
- **Stall Booking System**: Book and manage exhibition stalls with real-time availability
- **User Management**: Multi-role system (Admin, Stall Owner, Customer)
- **Payment Integration**: Secure payments through Razorpay gateway
- **Booking Management**: Track bookings, payments, and booking status
- **Feedback System**: Collect and manage user feedback

### 👥 User Roles
- **Admin**: Full system control, user management, payment reports
- **Stall Owner**: Create events, manage stalls, view bookings
- **Customer**: Browse events, book stalls, make payments, provide feedback

### 🔐 Security Features
- ASP.NET Core Identity for authentication and authorization
- Role-based access control
- Secure payment processing
- Data validation and protection

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 3.1 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Payment Gateway**: Razorpay
- **Frontend**: HTML5, CSS3, Bootstrap, JavaScript
- **ORM**: Entity Framework Core 3.1.32

## 📁 Project Structure

```
EasExpo/
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── BookingsController.cs
│   ├── EventsController.cs
│   ├── HomeController.cs
│   ├── StallOwnerController.cs
│   └── StallsController.cs
├── Models/               # Data Models
│   ├── ApplicationUser.cs
│   ├── Event.cs
│   ├── Stall.cs
│   ├── Booking.cs
│   ├── Payment.cs
│   ├── Feedback.cs
│   └── ViewModels/
├── Views/                # Razor Views
│   ├── Account/
│   ├── Admin/
│   ├── Bookings/
│   ├── Events/
│   ├── StallOwner/
│   └── Shared/
├── Services/             # Business Logic
│   ├── IRazorpayService.cs
│   └── RazorpayService.cs
├── Data/                 # Database Context & Seeding
│   └── DbSeeder.cs
└── Migrations/           # EF Core Migrations
```

## 🚀 Getting Started

### Prerequisites
- [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet/3.1)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (or SQL Server)
- [Visual Studio 2019+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Manan1107/EasExpo.git
   cd EasExpo
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   - Open `appsettings.json`
   - Update the `DefaultConnection` string if needed:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EasExpo;Trusted_Connection=True;MultipleActiveResultSets=true;"
     }
   }
   ```

4. **Configure Razorpay (Optional for payment testing)**
   - Update Razorpay credentials in `appsettings.json`:
   ```json
   {
     "Razorpay": {
       "KeyId": "your_razorpay_key_id",
       "KeySecret": "your_razorpay_key_secret"
     }
   }
   ```

5. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

6. **Build and run the application**
   ```bash
   dotnet build
   dotnet run
   ```

7. **Access the application**
   - Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

### SQL Server Connectivity Checklist
If you encounter database connection issues:

1. **Verify SQL Server LocalDB is installed and running**
   ```cmd
   SqlLocalDB info
   SqlLocalDB start MSSQLLocalDB
   ```

2. **Check connection string in appsettings.json**
3. **Ensure the database instance name is correct**
4. **Verify user permissions for database access**

## 🎮 Usage

### Default Seed Data
The application automatically seeds the database with:
- Default admin user
- Sample events and stalls
- User roles and permissions

### Key Features Walkthrough

1. **User Registration & Login**
   - Register as a new user or login with existing credentials
   - Role-based dashboard access

2. **Event Management (Stall Owners)**
   - Create new events with location, dates, and pricing
   - Manage event details and stall availability

3. **Stall Booking (Customers)**
   - Browse available events
   - Book stalls with secure payment
   - Track booking status

4. **Admin Panel**
   - Manage all users and roles
   - View payment reports
   - Handle stall owner applications

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Manan Javiya** - [Manan1107](https://github.com/Manan1107)

## 📞 Support

If you have any questions or need help with setup, please open an issue in the [GitHub repository](https://github.com/Manan1107/EasExpo/issues).

---

⭐ **If you found this project helpful, please give it a star!** ⭐
