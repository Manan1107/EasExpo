using System;
using System.Linq;
using System.Threading.Tasks;
using EasExpo.Models;
using EasExpo.Models.Constants;
using EasExpo.Models.Enums;
using EasExpo.Models.ViewModels.StallOwner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasExpo.Controllers
{
    [Authorize(Roles = RoleNames.StallOwner)]
    public class StallOwnerController : Controller
    {
        private readonly EasExpoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StallOwnerController(EasExpoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            var myStalls = await _context.Stalls.CountAsync(s => s.OwnerId == userId);
            var pendingBookings = await _context.Bookings
                .Include(b => b.Stall)
                .Where(b => b.Stall.OwnerId == userId && b.Status == BookingStatus.Pending)
                .CountAsync();

            var upcomingBookings = await _context.Bookings
                .Include(b => b.Stall)
                .Where(b => b.Stall.OwnerId == userId && b.Status == BookingStatus.Approved && b.StartDate >= DateTime.UtcNow.Date)
                .CountAsync();

            var revenue = await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Stall)
                .Where(p => p.Status == PaymentStatus.Completed && p.Booking.Stall.OwnerId == userId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var model = new StallOwnerDashboardViewModel
            {
                MyStallCount = myStalls,
                PendingBookings = pendingBookings,
                UpcomingBookings = upcomingBookings,
                TotalRevenue = Math.Round(revenue, 2)
            };

            return View(model);
        }

        public async Task<IActionResult> MyStalls()
        {
            var userId = _userManager.GetUserId(User);
            var stalls = await _context.Stalls
                .Where(s => s.OwnerId == userId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(stalls);
        }

        [HttpGet]
        public IActionResult CreateStall()
        {
            return View("StallForm", new OwnerStallFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStall(OwnerStallFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("StallForm", model);
            }

            var userId = _userManager.GetUserId(User);

            var stall = new Stall
            {
                Name = model.Name,
                Location = model.Location,
                Size = model.Size,
                RentPerDay = model.RentPerDay,
                Description = model.Description,
                OwnerId = userId,
                Status = model.Status
            };

            _context.Stalls.Add(stall);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Stall added successfully.";
            return RedirectToAction(nameof(MyStalls));
        }

        [HttpGet]
        public async Task<IActionResult> EditStall(int id)
        {
            var userId = _userManager.GetUserId(User);
            var stall = await _context.Stalls.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (stall == null)
            {
                return NotFound();
            }

            var model = new OwnerStallFormViewModel
            {
                Id = stall.Id,
                Name = stall.Name,
                Location = stall.Location,
                Size = stall.Size,
                RentPerDay = stall.RentPerDay,
                Description = stall.Description,
                Status = stall.Status
            };

            return View("StallForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStall(OwnerStallFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("StallForm", model);
            }

            var userId = _userManager.GetUserId(User);
            var stall = await _context.Stalls.FirstOrDefaultAsync(s => s.Id == model.Id && s.OwnerId == userId);
            if (stall == null)
            {
                return NotFound();
            }

            stall.Name = model.Name;
            stall.Location = model.Location;
            stall.Size = model.Size;
            stall.RentPerDay = model.RentPerDay;
            stall.Description = model.Description;
            stall.Status = model.Status;
            stall.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Stall updated successfully.";
            return RedirectToAction(nameof(MyStalls));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStall(int id)
        {
            var userId = _userManager.GetUserId(User);
            var stall = await _context.Stalls.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);
            if (stall == null)
            {
                return NotFound();
            }

            if (await _context.Bookings.AnyAsync(b => b.StallId == id))
            {
                TempData["Error"] = "Cannot delete a stall that has bookings.";
                return RedirectToAction(nameof(MyStalls));
            }

            _context.Stalls.Remove(stall);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Stall removed.";
            return RedirectToAction(nameof(MyStalls));
        }

        public async Task<IActionResult> Bookings()
        {
            var userId = _userManager.GetUserId(User);
            var bookings = await _context.Bookings
                .Include(b => b.Stall)
                .Include(b => b.Customer)
                .Where(b => b.Stall.OwnerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new OwnerBookingViewModel
                {
                    Id = b.Id,
                    StallName = b.Stall.Name,
                    CustomerName = b.Customer.FullName,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Status = b.Status,
                    PaymentStatus = b.PaymentStatus
                }).ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Stall)
                .FirstOrDefaultAsync(b => b.Id == id && b.Stall.OwnerId == userId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = BookingStatus.Approved;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.Stall.Status = StallStatus.Booked;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking approved.";
            return RedirectToAction(nameof(Bookings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Stall)
                .FirstOrDefaultAsync(b => b.Id == id && b.Stall.OwnerId == userId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = BookingStatus.Rejected;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.Stall.Status = StallStatus.Available;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking rejected.";
            return RedirectToAction(nameof(Bookings));
        }

        public async Task<IActionResult> Feedback()
        {
            var userId = _userManager.GetUserId(User);
            var feedback = await _context.Feedback
                .Include(f => f.Booking)
                .ThenInclude(b => b.Stall)
                .Include(f => f.Booking)
                .ThenInclude(b => b.Customer)
                .Where(f => f.Booking.Stall.OwnerId == userId)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new OwnerFeedbackViewModel
                {
                    StallName = f.Booking.Stall.Name,
                    CustomerName = f.Booking.Customer.FullName,
                    Rating = f.Rating,
                    Comments = f.Comments,
                    SubmittedAt = f.SubmittedAt
                }).ToListAsync();

            return View(feedback);
        }
    }
}
