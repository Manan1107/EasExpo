using EasExpo.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EasExpo.Models
{
    public class EasExpoDbContext : IdentityDbContext<ApplicationUser>
    {
        public EasExpoDbContext(DbContextOptions<EasExpoDbContext> options) : base(options)
        {
        }

        public DbSet<Stall> Stalls { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<StallOwnerApplication> StallOwnerApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Stall>()
                .HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Stall)
                .WithMany()
                .HasForeignKey(b => b.StallId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Feedback>()
                .HasOne(f => f.Booking)
                .WithMany()
                .HasForeignKey(f => f.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal support for SQLite
            builder.Entity<Stall>()
                .Property(s => s.RentPerDay)
                .HasConversion<double>();

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasConversion<double>();
        }
    }
}
