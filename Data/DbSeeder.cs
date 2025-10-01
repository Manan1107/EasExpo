using System;
using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasExpo.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var context = provider.GetRequiredService<EasExpoDbContext>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();

            await EnsureRolesAsync(roleManager);
            var admin = await EnsureAdminAsync(userManager);
            var owner = await EnsureStallOwnerAsync(userManager, context);
            var customer = await EnsureCustomerAsync(userManager);

            await SeedDomainDataAsync(context, owner, customer);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { RoleNames.Admin, RoleNames.StallOwner, RoleNames.Customer };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task<ApplicationUser> EnsureAdminAsync(UserManager<ApplicationUser> userManager)
        {
            const string email = "admin@easexpo.com";
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Super Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create default admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                await userManager.AddToRoleAsync(user, RoleNames.Admin);
            }

            return user;
        }

        private static async Task<ApplicationUser> EnsureStallOwnerAsync(UserManager<ApplicationUser> userManager, EasExpoDbContext context)
        {
            const string email = "owner@easexpo.com";
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Sample Stall Owner",
                    CompanyName = "Expo Traders",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Owner@123");
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create default stall owner: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, RoleNames.StallOwner))
            {
                await userManager.AddToRoleAsync(user, RoleNames.StallOwner);
            }

            if (!await context.StallOwnerApplications.AnyAsync(a => a.UserId == user.Id))
            {
                context.StallOwnerApplications.Add(new StallOwnerApplication
                {
                    UserId = user.Id,
                    Status = ApplicationStatus.Approved,
                    ReviewedAt = DateTime.UtcNow,
                    ReviewedBy = "Seeder",
                    DocumentUrl = "https://example.com/documents/owner-profile"
                });
                await context.SaveChangesAsync();
            }

            return user;
        }

        private static async Task<ApplicationUser> EnsureCustomerAsync(UserManager<ApplicationUser> userManager)
        {
            const string email = "customer@easexpo.com";
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Sample Customer",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Customer@123");
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create default customer: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, RoleNames.Customer))
            {
                await userManager.AddToRoleAsync(user, RoleNames.Customer);
            }

            return user;
        }

        private static async Task SeedDomainDataAsync(EasExpoDbContext context, ApplicationUser owner, ApplicationUser customer)
        {
            if (await context.Events.AnyAsync())
            {
                return;
            }

            var sampleEvent = new Event
            {
                Name = "EasExpo Launch Fair",
                Location = "Expo Center Block A",
                StartDate = DateTime.UtcNow.Date.AddDays(14),
                EndDate = DateTime.UtcNow.Date.AddDays(17),
                StallSize = "20x20 ft",
                SlotPrice = 2500,
                TotalSlots = 3,
                Description = "Experience the latest exhibitors in our flagship launch event.",
                OwnerId = owner.Id
            };

            context.Events.Add(sampleEvent);
            await context.SaveChangesAsync();

            var stall1 = new Stall
            {
                EventId = sampleEvent.Id,
                SlotNumber = 1,
                Name = "Launch Fair · Slot 1",
                Location = sampleEvent.Location,
                Size = sampleEvent.StallSize,
                RentPerDay = sampleEvent.SlotPrice,
                Description = "Prime spot near the main entrance.",
                Status = StallStatus.Available,
                OwnerId = owner.Id
            };

            var stall2 = new Stall
            {
                EventId = sampleEvent.Id,
                SlotNumber = 2,
                Name = "Launch Fair · Slot 2",
                Location = sampleEvent.Location,
                Size = sampleEvent.StallSize,
                RentPerDay = sampleEvent.SlotPrice,
                Description = "Ideal for boutique and lifestyle brands.",
                Status = StallStatus.Booked,
                OwnerId = owner.Id
            };

            var stall3 = new Stall
            {
                EventId = sampleEvent.Id,
                SlotNumber = 3,
                Name = "Launch Fair · Slot 3",
                Location = sampleEvent.Location,
                Size = sampleEvent.StallSize,
                RentPerDay = sampleEvent.SlotPrice,
                Description = "High footfall area perfect for F&B.",
                Status = StallStatus.Maintenance,
                OwnerId = owner.Id
            };

            await context.Stalls.AddRangeAsync(stall1, stall2, stall3);
            await context.SaveChangesAsync();

            sampleEvent.TotalSlots = 3;
            await context.SaveChangesAsync();

            var booking = new Booking
            {
                StallId = stall2.Id,
                CustomerId = customer.Id,
                StartDate = sampleEvent.StartDate,
                EndDate = sampleEvent.EndDate,
                Status = BookingStatus.Approved,
                PaymentStatus = PaymentStatus.Completed
            };

            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = stall2.RentPerDay * 3,
                Provider = "Razorpay",
                TransactionReference = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Status = PaymentStatus.Completed,
                ProcessedAt = DateTime.UtcNow
            };

            await context.Payments.AddAsync(payment);

            var feedback = new Feedback
            {
                BookingId = booking.Id,
                Rating = 5,
                Comments = "Great experience! Smooth booking process.",
                SubmittedAt = DateTime.UtcNow
            };

            await context.Feedback.AddAsync(feedback);

            await context.SaveChangesAsync();
        }
    }
}
